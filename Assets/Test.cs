using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    SimpleAnimator _controller;

    [Range(0f, 1f)]
    public float setNormalizedTime;
    public bool playForward;
    public bool playBackward;
    public bool stop;
    public AnimationClip clip;

    float _prevSetNormalizedTime;
    AnimationClip _prevClip;

    void Awake()
    {
        _controller = GetComponent<SimpleAnimator>();
    }

    void Update()
    {
        if (playForward)
        {
            playForward = false;

            _controller.Play(true);
        }

        if (playBackward)
        {
            playBackward = false;

            _controller.Play(false);
        }

        if (stop)
        {
            stop = false;

            _controller.Stop();
        }

        if (!Mathf.Approximately(setNormalizedTime, _prevSetNormalizedTime))
        {
            _controller.SetNormalizedTime(setNormalizedTime);
            _prevSetNormalizedTime = setNormalizedTime;
        }

        if (clip != _prevClip && clip != null)
        {
            _controller.SetClip(clip);
            _prevClip = clip;
        }
    }
}
