/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


// NOTE: ADAPTED FROM OCULUS SDK (SEE ABOVE)
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Oculus.Interaction.Samples
{
    public class UIButtonFlashAnimator : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("animator")]
        private Animator _animator;

        [SerializeField]
        [FormerlySerializedAs("overrideLayer")]
        private string _overrideLayer = "Selected Layer";

        [SerializeField]
        [FormerlySerializedAs("transitionDuration")]
        public float _transitionDuration = 0.2f;
        public float TransitionDuration
        {
            get => _transitionDuration;
            set => _transitionDuration = value;
        }

        [SerializeField]
        [FormerlySerializedAs("transitionCurve")]
        public AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve TransitionCurve
        {
            get => _transitionCurve;
            set => _transitionCurve = value;
        }

        [Space]
        [SerializeField, Optional(OptionalAttribute.Flag.DontHide)]
        [Tooltip("If provided, flashing is triggered when the toggle becomes true (toggle is reset to false after the flash).")]
        public Toggle _toggle;

        [SerializeField]
        [Tooltip("Fraction of transition duration used for the flash up (0→1); rest is flash down (1→0).")]
        [Range(0.1f, 0.9f)]
        private float _flashUpFraction = 0.35f;

        private bool _layerIsActive = false;
        private int _layerIndex = -1;
        protected bool _started;

        #region Editor events

        protected virtual void Reset()
        {
            _animator = this.GetComponent<Animator>();
            _toggle = this.GetComponent<Toggle>();
        }

        #endregion


        protected virtual void Start()
        {
            this.BeginStart(ref _started);

            this.AssertField(_animator, nameof(_animator));
            this.AssertField(_transitionCurve, nameof(_transitionCurve));

            _layerIndex = _animator.GetLayerIndex(_overrideLayer);

            this.AssertIsTrue(_layerIndex >= 0,
                whyItFailed: $"The Override Layer {_overrideLayer} could not be found in the Animator.",
                howToFix: $"Ensure you provide a layer that exists in the Animator");

            this.EndStart(ref _started);
        }

        protected virtual void OnEnable()
        {
            if (_started)
            {
                if (_layerIsActive)
                {
                    //After disabling the component. The state of the layers of the Animator is reset
                    //ensure we restore it accordingly.
                    _animator.SetLayerWeight(_layerIndex, 1.0f);
                }

                if (_toggle != null)
                {
                    _toggle.onValueChanged.AddListener(OnToggleValueChanged);
                }
            }
        }

        protected virtual void OnDisable()
        {
            if (_started)
            {
                if (_toggle != null)
                {
                    _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
                }
            }
        }

        private void OnToggleValueChanged(bool isOn)
        {
            if (isOn)
            {
                Flash();
                if (_toggle != null)
                    _toggle.SetIsOnWithoutNotify(false);
            }
        }

        /// <summary>
        /// Triggers a brief flash: animates the override layer weight 0 → 1 → 0, then leaves it at 0.
        /// </summary>
        public void Flash()
        {
            _layerIsActive = false;
            if (_transitionDuration > 0f)
            {
                StopAllCoroutines();
                StartCoroutine(FlashCoroutine());
            }
            else
            {
                _animator.SetLayerWeight(_layerIndex, 0f);
            }
        }

        public void SetOverrideLayerActive(bool active)
        {
            if (!active)
            {
                _layerIsActive = false;
                StopAllCoroutines();
                _animator.SetLayerWeight(_layerIndex, 0f);
                return;
            }
            Flash();
        }

        private IEnumerator FlashCoroutine()
        {
            float upDuration = _transitionDuration * Mathf.Clamp01(_flashUpFraction);
            float downDuration = _transitionDuration - upDuration;
            float startWeight = _animator.GetLayerWeight(_layerIndex);

            float startTime = Time.time;
            while (true)
            {
                float elapsed = Time.time - startTime;
                float weight;
                if (elapsed < upDuration && upDuration > 0f)
                {
                    float t = _transitionCurve.Evaluate(elapsed / upDuration);
                    weight = Mathf.Lerp(startWeight, 1f, t);
                }
                else if (downDuration > 0f)
                {
                    float downElapsed = elapsed - upDuration;
                    float t = downElapsed / downDuration;
                    if (t >= 1f)
                    {
                        weight = 0f;
                        _animator.SetLayerWeight(_layerIndex, weight);
                        yield break;
                    }
                    weight = Mathf.Lerp(1f, 0f, _transitionCurve.Evaluate(t));
                }
                else
                {
                    weight = 0f;
                    _animator.SetLayerWeight(_layerIndex, weight);
                    yield break;
                }
                _animator.SetLayerWeight(_layerIndex, weight);
                yield return null;
            }
        }

        #region Inject

        public void InjectAllAnimatorOverrideLayerWeigth(Animator animator, string overrideLayer)
        {
            InjectAnimator(animator);
            InjectOverrideLayer(overrideLayer);
        }

        public void InjectAnimator(Animator animator)
        {
            _animator = animator;
        }

        public void InjectOverrideLayer(string overrideLayer)
        {
            _overrideLayer = overrideLayer;
        }
        public void InjectOptionalToggle(Toggle toggle)
        {
            _toggle = toggle;
        }

        #endregion
    }
}
