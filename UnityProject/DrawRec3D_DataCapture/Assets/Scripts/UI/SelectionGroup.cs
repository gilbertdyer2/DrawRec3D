using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Oculus.Interaction.Samples
{
    public class SelectionGroup : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The ToggleGroup that manages the selection. If left empty, it will look in children.")]
        private ToggleGroup _toggleGroup;

        [SerializeField]
        [Tooltip("The list of toggles to manage. If empty, it finds all toggles under the ToggleGroup.")]
        private Toggle[] _toggles;

        /// <summary>
        /// Event triggered whenever a new item is clicked. Returns the index.
        /// </summary>
        public UnityEvent<int> WhenSelectionChanged;

        private int _selectedIndex = -1;
        public int SelectedIndex => _selectedIndex;

        private bool _started;

        protected virtual void Start()
        {
            if (_toggleGroup == null)
                _toggleGroup = GetComponentInChildren<ToggleGroup>(true);

            if (_toggleGroup != null)
                _toggleGroup.allowSwitchOff = false;

            if (_toggles == null || _toggles.Length == 0)
                _toggles = GetComponentsInChildren<Toggle>(true);

            InitializeToggles();

            // NEW: Force the first item to be ON if nothing is selected
            // This prevents the ToggleGroup from turning everything off.
            if (_toggles.Length > 0)
            {
                bool anyOn = _toggles.Any(t => t.isOn);
                if (!anyOn)
                {
                    _toggles[0].isOn = true;
                    _selectedIndex = 0;
                }
            }

            _started = true;
        }

        private void InitializeToggles()
        {
            for (int i = 0; i < _toggles.Length; i++)
            {
                int index = i; // Closure for the listener
                
                // Assign the group programmatically to be safe
                _toggles[i].group = _toggleGroup;

                _toggles[i].onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        HandleSelection(index);
                    }
                });

                // Set initial state
                if (_toggles[i].isOn)
                {
                    _selectedIndex = i;
                }
            }
        }

        private void HandleSelection(int index)
        {
            if (_selectedIndex != index)
            {
                _selectedIndex = index;
                WhenSelectionChanged?.Invoke(_selectedIndex);
            }
        }

        // Helper to manually set selection via code
        public void SelectItem(int index)
        {
            if (index >= 0 && index < _toggles.Length)
            {
                _toggles[index].isOn = true;
            }
        }
    }
}