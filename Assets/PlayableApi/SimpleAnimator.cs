using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public class SimpleAnimator : MonoBehaviour, IAnimationClipSource
{
    public bool isPlaying => enabled;
    public bool isForward => _targetTime > 0f;
    public AnimationClip currentClip => _playingClip != null ? _playingClip : GetDefaultClip();

    [SerializeField]
    AnimationClip[] _clips;

    Animator _animator;
    AnimationClip _playingClip;
    PlayableGraph _playableGraph;
    AnimationPlayableOutput _playableOutput;
    AnimationClipPlayable _clipPlayable;
    float _currentTime;
    float _targetTime;

    void Awake()
    {
        CheckInitialization();
    }

    public void SetClip(AnimationClip clip)
    {
        if (_playingClip == clip)
            return;

        float normalizedTime = 0f;
        if (_playingClip != null)
            normalizedTime = _currentTime / _playingClip.length;

        _currentTime = normalizedTime * clip.length;
        SetClipInternal(clip);
    }

    public void Play(bool forward)
    {
        CheckInitialization();

        if (_playingClip == null)
            SetClipInternal(GetDefaultClip());

        _targetTime = forward ? _playingClip.length : 0f;
        StartInternal();
    }

    public void Stop()
    {
        StopInternal();
    }

    public void SetNormalizedTime(float normalizedTime)
    {
        if (_playingClip == null)
            SetClipInternal(_clips[0]);

        bool animatorWasEnabled = _animator.enabled;
        if (!animatorWasEnabled)
            _animator.enabled = true;

        SetTime(_playingClip.length * normalizedTime);

        if (!animatorWasEnabled)
            _animator.enabled = false;
    }

    void StartInternal()
    {
        enabled = true;
        _animator.enabled = true;
    }

    void StopInternal()
    {
        enabled = false;
        _animator.enabled = false;
    }

    void CheckInitialization()
    {
        if (_animator != null)
            return;

        _animator = GetComponent<Animator>();

        StopInternal();

        _playableGraph = PlayableGraph.Create();
        _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        _playableOutput = AnimationPlayableOutput.Create(_playableGraph, "Animation", _animator);
    }

    void SetClipInternal(AnimationClip clip)
    {
        if (_playingClip == clip)
            return;

        _playingClip = clip;

        _clipPlayable = AnimationClipPlayable.Create(_playableGraph, _playingClip);
        _playableOutput.SetSourcePlayable(_clipPlayable);
    }

    void OnDestroy()
    {
        _playableGraph.Destroy();
    }

    void Update()
    {
        float time = Mathf.MoveTowards(_currentTime, _targetTime, Time.unscaledDeltaTime);
        SetTime(time);

        if (Mathf.Approximately(_currentTime, _targetTime))
            StopInternal();
    }

    void SetTime(float time)
    {
        _currentTime = time;
        _clipPlayable.SetTime(time);
        _playableGraph.Evaluate();
    }

    AnimationClip GetDefaultClip()
    {
        return _clips[0];
    }

    public void GetAnimationClips(List<AnimationClip> results)
    {
        results.AddRange(_clips);
    }

    void OnValidate()
    {
        if (_clips.Length == 0)
            _clips = new AnimationClip[1];
    }
}
