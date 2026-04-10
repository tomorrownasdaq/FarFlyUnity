using System.Collections;
using UnityEngine;

namespace HardCodeLab.TutorialMaster
{
    [HelpURL("https://hardcodelab.com/docs/tutorial-master/advanced/modules/effects#scale-pulsing-effect")]
    public class ScalePulsingEffect : Effect<ScalePulsingEffectSettings>
    {
        /// <summary>
        /// The default size of this Module before this effect occured.
        /// </summary>
        private Vector2 _defaultSize;

        /// <inheritdoc />
        protected override IEnumerator OnEffectBegin()
        {
            while (true)
            {
                float diffWidth = (Mathf.Sin(Time.time * EffectSettings.Speed) * EffectSettings.WidthRange - EffectSettings.WidthRange) / 2;
                float diffHeight = (Mathf.Sin(Time.time * EffectSettings.Speed) * EffectSettings.HeightRange - EffectSettings.HeightRange) / 2;

                RectTransform.sizeDelta = new Vector2(
                    _defaultSize.x + diffWidth,
                    _defaultSize.y + diffHeight
                );

                yield return new WaitForFixedUpdate();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _defaultSize = RectTransform.sizeDelta;
        }
    }
}
