using UnityEngine;
namespace ExtraEditMod
{
    public class AddLob
    {
        //Lボタン一度につき加算するLOBポイント
        private const int ADD_MONEY = 1;

        //キーを押しっぱなしにしている判定
        private const int KEEP_KEY_PUSH_COUNT = 30;

        /// <summary>
        /// LOBポイントを追加できるか？
        /// </summary>
        static bool m_enableAddLob = true;
        public static bool enableAddLob { get { return m_enableAddLob; } set { m_enableAddLob = value; } }

        /// <summary>
        /// LOBを追加できるか？
        /// </summary>
        public static bool isAddLob { get { return m_enableAddLob; } }

        //キーを押している数
        static int m_keyPushedCount = 0;

        //キーを押しているフラグ
        static bool m_isPushedFlag = false;

        //LOBポイント更新
        public void Update()
        {
            if (isAddLob &&( Input.GetKeyDown(KeyCode.L) || KEEP_KEY_PUSH_COUNT < m_keyPushedCount))
            {
                m_isPushedFlag = true;
                MoneyModel.instance.Add(ADD_MONEY);
            }
            if (Input.GetKeyUp(KeyCode.L))
            {
                m_keyPushedCount = 0;
                m_isPushedFlag = false;
            }
            if (m_isPushedFlag)
            {
                m_keyPushedCount++;
            }

        }
    }

}