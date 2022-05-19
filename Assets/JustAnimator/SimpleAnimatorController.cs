using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public interface ISimpleAnimatorController
{
    void Play(bool isForward);
    void SetNormalizedTime(float normalizedTime);
    void Stop();
}

[RequireComponent(typeof(Animator))]
public class SimpleAnimatorController : MonoBehaviour, IAnimationClipSource, ISimpleAnimatorController
{
    const string ClipNameInAnimatorController = "SimpleClip";
    static readonly int ForwardStateHash = Animator.StringToHash("ForwardState");
    static readonly int BackwardStateHash = Animator.StringToHash("BackwardState");

    public bool isPlaying => _isPlaying;
    public bool isForward => _isForward;

    //#pragma warning disable
    [SerializeField]
    AnimationClip _clip;
    [SerializeField]
    AnimationClip _clip2;
    [SerializeField]
    AnimatorController _defaultController;

    Animator _animator;
    AnimatorOverrideController _controller;
    bool _isInitialized;
    bool _isPlaying;
    bool _isForward = true;
    float _normalizedTime;

    void Awake()
    {
        CheckInitialization();
    }

    public void Play(bool isForward)
    {
        CheckInitialization();

        if (_isPlaying && _isForward == isForward)
            return;

        _isForward = isForward;
        _isPlaying = true;
        _animator.enabled = true;

        PlayAnimator();
    }

    public void SetNormalizedTime(float normalizedTime)
    {
        CheckInitialization();

        _normalizedTime = normalizedTime;
        PlayAnimator();

        if (!_isPlaying)
        {
            _animator.Update(0f);
            StopInternal();
        }
    }

    void PlayAnimator()
    {
        int directionStateHash = _isForward ? ForwardStateHash : BackwardStateHash;
        float startNormalizedTime = _isForward ? _normalizedTime : 1f - _normalizedTime;
        _animator.Play(directionStateHash, -1, startNormalizedTime);
    }

    public void Stop()
    {
        StopInternal();
    }

    void StopInternal()
    {
        _isPlaying = false;
        _animator.enabled = false;
    }


    void CheckInitialization()
    {
        if (_isInitialized)
            return;

        _animator = GetComponent<Animator>();
        _animator.enabled = false;

        _controller = new AnimatorOverrideController(_defaultController)
        {
            name = "runtimeCreatedController",
            [ClipNameInAnimatorController] = _clip
        };
/*
        var playableGraph = PlayableGraph.Create();
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());
        var clipPlayable = AnimationClipPlayable.Create(playableGraph, _clip);
        playableOutput.SetSourcePlayable(clipPlayable);
        playableGraph.Play();
        clipPlayable.Pause();

        clipPlayable.SetTime();*/

        _animator.runtimeAnimatorController = _controller;
        SimpleAnimatorBehaviour[] behaviours = _animator.GetBehaviours<SimpleAnimatorBehaviour>();
        foreach (SimpleAnimatorBehaviour behaviour in behaviours)
            behaviour.onProgress += OnAnimationProgress;

        _isInitialized = true;
    }

    void OnAnimationProgress(float normalizedTime)
    {
        if (!_isForward)
            normalizedTime = 1f - normalizedTime;

        _normalizedTime = Mathf.Clamp01(normalizedTime);
        if ((_isForward && _normalizedTime >= 1f) || (!_isForward && _normalizedTime <= 0f))
            StopInternal();
    }

    public void GetAnimationClips(List<AnimationClip> results)
    {
        if (_clip != null)
            results.Add(_clip);

        if (_clip2 != null)
            results.Add(_clip2);
    }
}
