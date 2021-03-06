﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ILModWriter
{
    class ILModWriter
    {
        static ILModWriter instance = new ILModWriter();

        /// <summary>
        /// start program
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            instance.MainProsess();
        }

        /// <summary>
        /// メイン
        /// </summary>
        void MainProsess()
        {
            //ilファイルを行単位で取得する
            var list = IlLineList(@"Assembly-CSharp.il");

            //自作modのDllのバージョンを追加する
            AddExtraEditModDllVersion(list);

            /// イントロプレイヤーのAwake更新
            AddInitForIntroPlayerAwake(list);

            //ギフトを必ず手に入れる機能入れるためgetRandomしてる箇所を書き換える
            CnangeFinishWorkSuccessfully(list);

            //CreatureGenerateModel::SetCreature を書き換え
            CnangeSetCreature(list);

            //CommandWindow::OnClick を書き換え
            CnangeCommandWindowOnClick(list);

            //現状のStringのリストをファイルとして書き出す
            IlListWriter(@"_Assembly-CSharp.il", list);
        }


        /// <summary>
        /// modのバージョンを追加する
        /// </summary>
        /// <param name="list"></param>
        void AddExtraEditModDllVersion(List<string> list)
        {
            string search = @".assembly 'Assembly-CSharp'";

            int baseIndex = IndexOfForList(0, search, list);

            list.Insert(baseIndex, @".assembly extern ExtraEditMod");
            list.Insert(++baseIndex, @"{");
            list.Insert(++baseIndex, @"  .ver 1:0:0:0");
            list.Insert(++baseIndex, @"}");
        }

        /// <summary>
        /// イントロプレイヤーのAwake更新
        /// </summary>
        void AddInitForIntroPlayerAwake(List<string> list)
        {
            string search = @"// end of method IntroPlayer::Awake";
            int baseIndex = IndexOfForList(0, search, list);

            int labelNum = GetLabelNumber(list[baseIndex - 1]);

            list[baseIndex - 1] = GetLabel(labelNum) + @"call       void [ExtraEditMod]ExtraEditMod.ExtraEditMod::Init()";
            list.Insert(baseIndex, GetLabel(labelNum + 5) + "ret");
        }
        /// <summary>
        /// タイトルのAwake更新
        /// </summary>
        void AddInitForTitleScriptAwake(List<string> list)
        {
            string search = @"// end of method NewTitleScript::Awake";
            int baseIndex = IndexOfForList(0, search, list);

            int labelNum = GetLabelNumber(list[baseIndex - 1]);

            list[baseIndex - 1] = GetLabel(labelNum) + @"call       void [ExtraEditMod]ExtraEditMod.ExtraEditMod::Init()";
            list.Insert(baseIndex, GetLabel(labelNum + 5) + "ret");
        }

        /// <summary>
        /// ギフトを必ず手に入れる機能入れるためgetRandomしてる箇所を書き換える
        /// </summary>
        /// <param name="list"></param>
        void CnangeFinishWorkSuccessfully(List<string> list)
        {
            string search = @"// end of method UseSkill::FinishWorkSuccessfully";
            int baseIndex = IndexOfForList(0, search, list);
            var limitIndex = baseIndex;

            int minus = -1;
            while (list[baseIndex + minus].IndexOf(@"FinishWorkSuccessfully() cil managed") < 0)
            {
                --minus;
            }
            baseIndex = baseIndex + minus;
            int index = 1;

            while (baseIndex + index < limitIndex)
            {
                if (0 <= list[baseIndex + index].IndexOf(@"float32 [UnityEngine.CoreModule]UnityEngine.Random::get_value()"))
                {
                    int labelNum = GetLabelNumber(list[baseIndex + index]);
                    list[baseIndex + index] = GetLabel(labelNum) + @"call       float32 [ExtraEditMod]ExtraEditMod.ExtraEditMod::GetRamdomeValueOrZeroByAlwaysGetGiftFlag()";
                    break;
                }
                index++;
            }

        }

        /// <summary>
        /// CreatureGenerateModel::SetCreature を書き換え
        /// </summary>
        /// <param name="list"></param>
        void CnangeSetCreature(List<string> list)
        {
            string search = @"// end of method CreatureGenerateModel::SetCreature";
            int baseIndex = IndexOfForList(0, search, list);
            int minus = -1;
            while (list[baseIndex + minus].IndexOf(@"SetCreature() cil managed") < 0)
            {
                --minus;
            }
            baseIndex = baseIndex + minus;
            while (list[baseIndex + 2].IndexOf(@"// end of method CreatureGenerateModel::SetCreature") < 0)
            {
                list.RemoveAt(baseIndex + 2);
            }

            list.Insert(baseIndex + 2, @"    IL_0000:  ldarg.0");
            list.Insert(baseIndex + 3, @"    IL_0001:  call       void [ExtraEditMod]ExtraEditMod.ExtraEditMod::SetCreature(object)");
            list.Insert(baseIndex + 4, @"    IL_0006:  ret");
        }

        /// <summary>
        /// CommandWindow::OnClick を書き換え
        /// </summary>
        /// <param name="list"></param>
        void CnangeCommandWindowOnClick(List<string> list)
        {
            string search = @"// end of method CommandWindow::OnClick";
            int baseIndex = IndexOfForList(0, search, list);
            int minus = -1;
            while (list[baseIndex + minus].IndexOf(@"OnClick(class AgentModel actor) cil managed") < 0)
            {
                --minus;
            }
            baseIndex = baseIndex + minus;
            while (list[baseIndex + 2].IndexOf(@"// end of method CommandWindow::OnClick") < 0)
            {
                list.RemoveAt(baseIndex + 2);
            }

            list.Insert(baseIndex + 2, @"    IL_0000: ldarg.1");
            list.Insert(baseIndex + 3, @"    IL_0001: call void [ExtraEditMod]ExtraEditMod.ExtraEditMod::OnClick(class AgentModel)");
            list.Insert(baseIndex + 4, @"    IL_0006:  ret");
        }

        /// <summary>
        /// ラベルの№取得
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        int GetLabelNumber(string line)
        {
            int numberIndex = line.IndexOf("IL") + 3;
            string numStr = line.Substring(numberIndex, 4);
            return Convert.ToInt32(numStr, 16);
        }


        /// <summary>
        /// 番号からラベルを取得する
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        string GetLabel(int number)
        {
            string label = "    IL_" + number.ToString("x4") + ":  ";
            return label;
        }


        /// <summary>
        /// ラベル取得
        /// </summary>
        /// <param name="line"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        string GetLabelId(string line, string command)
        {
            return line.Substring(0, line.IndexOf(command));
        }


        /// <summary>
        /// 特定の文字列が含む行を探す
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="search"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        int IndexOfForList(int startIndex, string search, List<string> target)
        {
            for (int i = startIndex; i < target.Count; i++)
            {
                if (0 <= target[i].IndexOf(search))
                {
                    return i;
                }
            }

            return -1;
        }


        /// <summary>
        /// 特定の文字列が完全一致する行を探す
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="search"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        int EqualStringForList(int startIndex, string search, List<string> target)
        {
            for (int i = startIndex; i < target.Count; i++)
            {
                if (target[i] == search)
                {
                    return i;
                }
            }

            return -1;
        }



        /// <summary>
        /// リストファイル書き込み
        /// </summary>
        /// <param name="list"></param>
        void IlListWriter(string path, List<string> list)
        {
            Encoding utf8Enc = Encoding.UTF8;
            StreamWriter writer =
              new StreamWriter(path, false, utf8Enc);

            foreach (var str in list)
            {
                writer.WriteLine(str);
            }
            writer.Close();
        }


        /// <summary>
        /// ilファイルを行単位で取得する
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        List<string> IlLineList(string path)
        {
            List<string> list = new List<string>();         //空のListを作成する

            using (System.IO.StreamReader file = new StreamReader(path, System.Text.Encoding.UTF8))
            {
                string line = "";
                // test.txtを1行ずつ読み込んでいき、末端(何もない行)までwhile文で繰り返す
                while ((line = file.ReadLine()) != null)
                {
                    list.Add(line);
                }
            }
            return list;
        }
    }
}
