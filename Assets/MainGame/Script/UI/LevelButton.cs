using UnityEngine;
using UnityEngine.UI;

namespace Tag.Block
{
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private Text levelNumberText;

        public void Setup(int level)
        {
            levelNumberText.text = level.ToString();
        }
    }
}