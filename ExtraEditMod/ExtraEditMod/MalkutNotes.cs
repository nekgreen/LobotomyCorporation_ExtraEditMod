using UnityEngine;
namespace ExtraEditMod
{
    /// <summary>
    /// 今日稼いだ経験値を表示
    /// </summary>
    public class MalkutNotes
    {
        /// <summary>
        /// 必ずギフトが手に入るか？
        /// </summary>
        static bool m_enableMalkutNotes = false;
        public static bool enableMalkutNotes { get { return m_enableMalkutNotes; } set { m_enableMalkutNotes = value; } }

        /// <summary>
        /// 今日稼いだ経験値を表示
        /// </summary>
        /// <returns></returns>
        public void OnGUI()
        {
            if (enableMalkutNotes &&
                AgentInfoWindow.currentWindow != null &&
                AgentInfoWindow.currentWindow.IsEnabled &&
                GameManager.currentGameManager.state != GameState.STOP)
            {
                AgentModel target = AgentInfoWindow.currentWindow.CurrentAgent;
                if (target != null)
                {
                    var width = 300.0f;
                    var labelHeight = 20.0f;
                    var x = Screen.width - 300.0f;
                    var y = 270.5f + 55;
                    GUI.Box(new Rect(x, y, width, 200.0f), "Malkut Notes");

                    string[] value = new string[]
                    {
                       target.name + " EXP acquired today",
                       "===========",
                       "Hp:"+target.primaryStatExp.hp,
                       "mental:"+target.primaryStatExp.mental,
                       "work:"+target.primaryStatExp.work,
                       "battle:"+target.primaryStatExp.battle,
                    };

                    for (int i = 0; i < value.Length; i++)
                    {
                        y += labelHeight;
                        GUI.Label(new Rect(x, y, width, 20.0f), value[i]);
                    }
                }
            }
        }
    }
}