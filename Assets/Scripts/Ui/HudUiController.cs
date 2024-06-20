using UnityEngine;
using UnityEngine.UIElements;

namespace Ui
{
    public class HudUiController : MonoBehaviour
    {
        private VisualElement _root;

        private VisualElement _hpDisplay;
        private Label _hpLabel;

        private void Awake()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            _hpDisplay = _root.Q<VisualElement>("HPDisplay");
            _hpLabel = _root.Q<Label>("HPLabel");
        }

        public void SetHp(int hp)
        {
            _hpDisplay.style.width = Length.Percent(hp);
            _hpLabel.text = hp.ToString();
        }
    }
}