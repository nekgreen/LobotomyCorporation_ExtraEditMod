using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using CreatureGenerate;
using UnityEngine.SceneManagement; // コレ重要
using System.Text;
using UnityEngine;
using CommandWindow;

namespace ExtraEditMod
{

    /// <summary>
    /// 作業ログ
    /// </summary>
    public class WorkLog
    {
        /// <summary>
        /// 開始時間
        /// </summary>
        public float timer = 0.0f;

        /// <summary>
        /// 仕事のスキル
        /// </summary>
        public long skill;

        /// <summary>
        /// 仕事の対象
        /// </summary>
        public UnitModel target = null;

        /// <summary>
        /// エージェント
        /// </summary>
        public AgentModel actor;

        /// <summary>
        /// スキルの情報
        /// </summary>
        public SkillTypeInfo skillInfo;

        //削除する
        public bool m_deleteFlag;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_timer"></param>
        /// <param name="_skill"></param>
        /// <param name="_target"></param>
        public WorkLog(float _timer, long _skill, UnitModel _target, AgentModel _actor)
        {
            timer = _timer;
            skill = _skill;
            target = _target;
            actor = _actor;
            m_deleteFlag = false;
            skillInfo = SkillTypeList.instance.GetData(skill);
        }

        public string toString()
        {
            return string.Format("時間:{0} skill:{1}, target:{2} agent:{3}  state:{4} cancel:{5}\n", timer, skill, target.GetUnitName(), actor.GetUnitName(), actor.GetState().ToString(), actor.canCancelCurrentWork);
        }
    }


    /// <summary>
    /// タスクマネージャー
    /// </summary>
    public class TaskManager
    {
        /// <summary>
        /// タスクマネージャーが有効か？
        /// </summary>
        static bool m_enableTaskManager = false;
        public static bool enableTaskManager { get { return m_enableTaskManager; } set { m_enableTaskManager = value; } }


        #region メンバ変数

        /// <summary>
        /// 行動記録を取るかどうか
        /// </summary>
        bool m_recodeWorkLog = false;

        /// <summary>
        /// 行動記録を再生するかどうか
        /// </summary>
        bool m_playWorkLog = false;


        /// <summary>
        /// 折り畳むフラグ
        /// </summary>
        bool m_FoldUpFlag = false;

        /// <summary>
        /// 行動記録の再生index
        /// </summary>
        int m_workLogIndex = 0;

        /// <summary>
        /// 経過時間
        /// </summary>
        float m_recodeElapsed = 0.0f;

        /// <summary>
        /// タスクの経過時間
        /// </summary>
        float m_workLogElapsed = 0.0f;

        /// <summary>
        /// 作業ログのリスト
        /// </summary>
        List<WorkLog> m_workLogList = new List<WorkLog>();

        /// <summary>
        /// タスクリストに追加する前の一時的なリスト
        /// </summary>
        List<WorkLog> m_tempLogList = new List<WorkLog>();

        //状態を表示する文字列
        string m_statusText = "";

        //ログ用の文字列
        string m_logText = "";

        /// <summary>
        /// デバッグ用のログ
        /// </summary>
        public string m_debugLog = "";

        /// <summary>
        /// UIのメインウィンドウ用のスクロールの現在位置
        /// </summary>
        Vector2 mainScrollViewVector = Vector2.zero;

        /// <summary>
        /// スクロールビューの全体のRect範囲。position以上であれば、スクロールバーを操作して表示する 
        /// </summary>
        Rect scrollViewAllRect = new Rect(0, 0, 100, BUTTON_HEIGHT_INTARVAL * (50));

        /// <summary>
        /// ログの状態
        /// </summary>
        LogStatus logStatus = LogStatus.Init;

        /// <summary>
        /// ループの回数
        /// </summary>
        int m_loopCount = 0;

        #endregion

        #region プロパティ

        /// <summary>
        /// ログ記録を開始してからの経過時間
        /// </summary>
        public float recodeElapsed
        {
            get
            {
                return m_recodeElapsed;
            }
        }

        /// <summary>
        /// ログを記録するフラグ
        /// </summary>
        public bool recodeWorkLog
        {
            get
            {
                return m_recodeWorkLog;
            }
        }
        #endregion

        #region 定数

        //ログ状態
        enum LogStatus
        {
            Init = 0,
            Rec,
            Stop,
            Play,
            Pause,
        }

        string[] LogStatusString = new string[] { "タスク登録待ち", "タスク登録中", "タスク開始待ち", "タスク実行中", "タスク一時停止" };


        /// <summary>
        /// 高さのマージン
        /// </summary>
        const int UI_MARGIN = 10;

        /// <summary>
        /// ラベルの高さ
        /// </summary>
        const int LABEL_HEIGHT = 50;

        /// <summary>
        /// ボタンの高さ
        /// </summary>
        const int BUTTON_WIDTH = 100;
        const int BUTTON_HEIGHT = 50;

        public const int IMAGE_X = 5;
        public const int IMAGE_SIZE = 80;

        const int BUTTON_HEIGHT_INTARVAL = IMAGE_SIZE+10;
        

        public const int UI_BETWEEN_HEIGHT = 10;
        public const int UI_BETWEEN_WIGHT = 5;
        #endregion


        public void Update()
        {
            var manager = GameManager.currentGameManager;
            var window = CommandWindow.CommandWindow.CurrentWindow;
            //準備が出来てなかったり、この機能が使用できない状態の時
            if (manager == null || window == null || !manager.ManageStarted || !enableTaskManager)
            {
                if (logStatus != LogStatus.Init)
                {
                    AllReset();
                    TaskReset();
                    logStatus = LogStatus.Init;
                    m_statusText = "";
                }
                return;
            }

            bool isSort = false;
            //一時的なリストにあるものをチェックし、指示がキャンセルされてないか確認する
            for (int i = 0; i < m_tempLogList.Count; i++)
            {
                if (m_tempLogList[i].actor.GetState() == AgentAIState.MANAGE && !m_tempLogList[i].m_deleteFlag)
                {
                    //AgentAIState.MANAGE 且つ　キャンセルできない で　タスク登録
                    if (!m_tempLogList[i].actor.canCancelCurrentWork)
                    {
                        isSort = true;
                        m_workLogList.Add(m_tempLogList[i]);
                        m_tempLogList.RemoveAt(i);
                        i--;
                    }
                }
                else //ステータスが変わったものはキャンセルとして扱う
                {
                    m_tempLogList.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < m_workLogList.Count; i++)
            {
                if(m_workLogList[i].m_deleteFlag)
                {
                    m_workLogList.RemoveAt(i);
                    i--;
                }
            }

            //時間を整数にしてソートする
            if (isSort)
            {
                m_workLogList.Sort((a, b) => (int)(a.timer * 1000) - (int)(b.timer * 1000));
            }


            if (m_recodeWorkLog)
            {
                m_recodeElapsed += Time.deltaTime;

                m_statusText = "経過時間:" + m_recodeElapsed + "\n";
                m_logText = "";
                for (int i = 0; i < m_workLogList.Count; i++)
                {
                    m_logText += m_workLogList[i].toString();
                }
            }
            else
            {
                m_recodeElapsed = 0.0f;
            }

            if (m_playWorkLog && 0 < m_workLogList.Count)
            {
                if (window.IsEnabled)
                {
                    m_statusText = "ウインドウの閉じ待ち";
                    return;
                }


                //var isCreatureEscaped = GameStatusUI.GameStatusUI.Window.sceneController.IsCreatureEscaped;

                //if (isCreatureEscaped)
                //{
                //    m_statusText = "収容違反が発生したため一時停止";
                //    m_playWorkLog = false;
                //    logStatus = LogStatus.Pause;
                //    return;
                //}
                  


                m_statusText = (m_loopCount+1)+"巡目　経過時間:" + m_workLogElapsed + "\n";
                m_workLogElapsed += Time.deltaTime;

                if (m_workLogList.Count <= m_workLogIndex)
                {
                    //最後のエージェントの仕事が終わったらリセットする
                    if (AllTaskListCheck())
                    {
                        m_loopCount++;
                        TaskReset();
                        return;
                    }
                }
                
                if (m_workLogIndex < m_workLogList.Count && m_workLogList[m_workLogIndex].timer < m_workLogElapsed)
                {
                    //状態異常や別の仕事を割り振っている場合
                    if (!isNormalStatus(m_workLogList[m_workLogIndex].actor) || m_workLogList[m_workLogList.Count - 1].actor.GetState() != AgentAIState.IDLE)
                    {
                        m_statusText = m_workLogList[m_workLogIndex].actor._agentName.GetName() + "が、指定した時間に作業できない状態だったため、タスク中止";
                        m_playWorkLog = false;
                        TaskReset();
                        logStatus = LogStatus.Stop;
                        return;
                    }
                    CommandWindow.CommandWindow.CreateWindow(CommandType.Management, m_workLogList[m_workLogIndex].target);
                    window.SelectedWork = m_workLogList[m_workLogIndex].skill;

                    ExtraEditMod.OnClick(m_workLogList[m_workLogIndex].actor);
                    m_workLogIndex++;
                }
            }

        }

        private bool AllTaskListCheck()
        {
            for (int i = 0; i < m_workLogList.Count; i++)
            {
                var creatureModel = m_workLogList[i].target as CreatureModel;

                if (creatureModel == null) return false;

                if (0.0f < creatureModel.feelingStateRemainTime)
                {
                    m_statusText = "ループ待機中:"+creatureModel.GetUnitName() + "のクールタイム待ち";
                    return false;
                }
                else if (m_workLogList[i].actor.GetState() != AgentAIState.IDLE)
                {
                    m_statusText = "ループ待機中:" + m_workLogList[i].actor.GetUnitName()+"の作業完了待ち";
                    return false;
                }
            }
            return true;
        }



        /// <summary>
        /// 全て消す
        /// </summary>
        public void AllReset()
        {
            if (0 < m_workLogList.Count)
            {
                m_workLogList.Clear();
            }

            if (0 < m_tempLogList.Count)
            {
                m_tempLogList.Clear();
            }
            m_recodeWorkLog = false;
            m_playWorkLog = false;
        }

        /// <summary>
        /// タスクのリセット
        /// </summary>
        public void TaskReset()
        {
            m_workLogElapsed = 0.0f;
            m_workLogIndex = 0;
        }

        /// <summary>
        /// 指定したエージェントの状態は正常か？
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool isNormalStatus(AgentModel model)
        {
            return !(model.IsCrazy() || model.IsPanic() || model.IsDead());
        }

        /// <summary>
        /// ログを登録する
        /// </summary>
        /// <param name="log"></param>
        public void AddWorkLog(WorkLog log)
        {
            if (m_recodeWorkLog)
            {
                m_tempLogList.Add(log);
            }
        }
        /// <summary>
        /// 今日稼いだ経験値を表示
        /// </summary>
        /// <returns></returns>
        public void OnGUI()
        {

            var manager = GameManager.currentGameManager;
            var window = CommandWindow.CommandWindow.CurrentWindow;

            if (enableTaskManager &&
                !(manager == null || window == null || !manager.ManageStarted))
            {
                int width = 300;
                int height = m_FoldUpFlag ? 20 : 400;

                int x = Screen.width - (width);
                int y = Screen.height - (height);

                GUI.Box(new Rect(x, y, width, height), "Task Manager");

                //閉じるボタン
                if (GUI.Button(new Rect(x + 5, y + 5, 15, 15), m_FoldUpFlag ? "△" : "▽"))
                {
                    m_FoldUpFlag = !m_FoldUpFlag;
                }
                //閉じてるならここで描画は終了
                if (m_FoldUpFlag) return;

                y += UI_MARGIN*3;

                //ボタンとステータス表示
                y = OnGUIButtons(x, y, width);

                // position : 表示位置
                Rect scrollViewRect = new Rect(x, y, width - 30, BUTTON_HEIGHT_INTARVAL * 4 + 10);

                //スクロールビュー配置
                mainScrollViewVector = GUI.BeginScrollView(scrollViewRect, mainScrollViewVector, scrollViewAllRect);

                y += BUTTON_HEIGHT_INTARVAL * 4 + 10 + UI_BETWEEN_HEIGHT * 2;

                int buttonAmount = 0;

                //スクロールの中身
                for (int i = 0; i < m_tempLogList.Count; i++)
                {
                    OnGUITaskLog(m_tempLogList[i], buttonAmount,"タスク設定中",width);
                    buttonAmount++;
                }

                if (0 < m_tempLogList.Count)
                {
                    GUI.Label(new Rect(IMAGE_X  + UI_MARGIN, (buttonAmount * BUTTON_HEIGHT_INTARVAL) + 20, width, 20), "-----------------------------");
                    buttonAmount++;
                }


                //スクロールの中身
                for (int i = 0; i < m_workLogList.Count; i++)
                {                   
                    string state = !m_playWorkLog ? "タスク登録完了" : i == m_workLogIndex ? "現在のタスク" : i < m_workLogIndex ? "実行済み" : "実行待ち";
                    OnGUITaskLog(m_workLogList[i], buttonAmount, state, width);
                    buttonAmount++;
                }
                GUI.EndScrollView();
            }
        }

        /// <summary>
        /// タスクログを表示
        /// </summary>
        /// <param name="creatureModel"></param>
        /// <param name="buttonAmount"></param>
        /// <param name="stateString"></param>
        void OnGUITaskLog(WorkLog workLog, int buttonAmount, string stateString, int width)
        {
            var creatureModel = workLog.target as CreatureModel;

            if (creatureModel == null) return;

            if (GUI.Button(new Rect(IMAGE_X - 5, (buttonAmount * BUTTON_HEIGHT_INTARVAL), width + 5, IMAGE_SIZE + 5), ""))
            {
                workLog.m_deleteFlag = true;
            }

            GUI.DrawTexture(new Rect(IMAGE_X, (buttonAmount * BUTTON_HEIGHT_INTARVAL), IMAGE_SIZE, IMAGE_SIZE), ExtraEditMod.m_creatureSprite[creatureModel.metadataId].texture);

            //状態表示
            GUI.Label(new Rect(IMAGE_X + IMAGE_SIZE + UI_MARGIN, (buttonAmount * BUTTON_HEIGHT_INTARVAL), width, 20), "職員:" + workLog.actor._agentName.GetName());
            GUI.Label(new Rect(IMAGE_X + IMAGE_SIZE + UI_MARGIN, (buttonAmount * BUTTON_HEIGHT_INTARVAL) + 20, width, 20), "作業:" + workLog.skillInfo.name);
            //GUI.Label(new Rect(IMAGE_X + IMAGE_SIZE + UI_MARGIN, (buttonAmount * BUTTON_HEIGHT_INTARVAL) + 40, width, 20), "状態:" + workLog.actor.GetState().ToString());
            GUI.Label(new Rect(IMAGE_X + IMAGE_SIZE + UI_MARGIN, (buttonAmount * BUTTON_HEIGHT_INTARVAL) + 40, width, 20), "指示時間:" + workLog.timer);
            GUI.Label(new Rect(IMAGE_X + IMAGE_SIZE + UI_MARGIN, (buttonAmount * BUTTON_HEIGHT_INTARVAL) + 60, width, 20), stateString);

        }

        /// <summary>
        /// ボタンを表示する
        /// </summary>
        int OnGUIButtons(int x, int y,int width)
        {


            //状態表示
            GUI.Label(new Rect(x, y, width, LABEL_HEIGHT), "状態:" + LogStatusString[(int)logStatus]+"\n"+m_statusText);

            y += UI_MARGIN + LABEL_HEIGHT;

            var buttonX = x + UI_MARGIN;


            switch (logStatus)
            {
                case LogStatus.Init:
                    if (GUI.Button(new Rect(buttonX, y, BUTTON_WIDTH, BUTTON_HEIGHT), "記録開始"))
                    {
                        m_recodeWorkLog = true;
                        logStatus = LogStatus.Rec;
                    }
                    break;
                case LogStatus.Rec:
                    if (GUI.Button(new Rect(buttonX, y, BUTTON_WIDTH, BUTTON_HEIGHT), "記録完了"))
                    {
                        m_recodeWorkLog = false;
                        logStatus = LogStatus.Stop;
                    }
                    break;
                case LogStatus.Stop:
                    if (GUI.Button(new Rect(buttonX, y, BUTTON_WIDTH, BUTTON_HEIGHT), "タスク開始"))
                    {
                        m_loopCount = 0;
                        m_playWorkLog = true;
                        logStatus = LogStatus.Play;
                    }
                    if (GUI.Button(new Rect(buttonX + (BUTTON_WIDTH + UI_MARGIN), y, BUTTON_WIDTH, BUTTON_HEIGHT), "タスク全消去"))
                    {
                        AllReset();
                        TaskReset();
                        logStatus = LogStatus.Init;
                    }
                    break;
                case LogStatus.Play:
                    if (GUI.Button(new Rect(buttonX, y, BUTTON_WIDTH, BUTTON_HEIGHT), "タスク一時停止"))
                    {
                        m_playWorkLog = false;
                        logStatus = LogStatus.Pause;
                    }
                    break;
                case LogStatus.Pause:
                    if (GUI.Button(new Rect(buttonX, y, BUTTON_WIDTH, BUTTON_HEIGHT), "タスク再開"))
                    {
                        m_playWorkLog = true;
                        logStatus = LogStatus.Play;
                    }
                    if (GUI.Button(new Rect(buttonX + (BUTTON_WIDTH + UI_MARGIN), y, BUTTON_WIDTH, BUTTON_HEIGHT), "タスク全消去"))
                    {
                        AllReset();
                        TaskReset();
                        logStatus = LogStatus.Init;
                    }
                    break;

            }
            y += UI_MARGIN + BUTTON_HEIGHT;

            return y;
        }
    }
}
