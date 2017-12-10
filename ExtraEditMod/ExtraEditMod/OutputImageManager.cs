using System;
using System.Collections.Generic;
using CreatureGenerate;
using UnityEngine;
using System.IO;

/// <summary>
/// リソースを取り出す機能
/// </summary>
namespace ExtraEditMod
{
    public class OutputImageManager
    {
        /// <summary>
        /// アブノーマリティの画像を取り出す
        /// </summary>
        public static void OutputAbnormaltyImage()
        {
            foreach (var metaID in CreatureGenerateInfo.all)
            {
                var typeInfo = CreatureTypeList.instance.GetData(metaID);
                var fileName = "" + metaID + ".png";
                string str = typeInfo.portraitSrcForcely;
                var img = Resources.Load<Sprite>(str);

                var tempTexture = RenderTexture.GetTemporary(img.texture.width, img.texture.height);
                Graphics.Blit(img.texture, tempTexture);
                // ReadPixelsで直前のレンダリング結果を読み込める
                var copy = new Texture2D(tempTexture.width, tempTexture.height);
                copy.ReadPixels(new Rect(0, 0, tempTexture.width, tempTexture.height), 0, 0);
                RenderTexture.ReleaseTemporary(tempTexture);


                // pngファイル保存.
                try
                {
                    byte[] pngData = copy.EncodeToPNG();   // pngのバイト情報を取得.
                    File.WriteAllBytes(Application.dataPath + ",,/../AbnormalityImages/" + fileName, pngData);
                }
                catch (Exception e)
                {
                    ExtraEditMod.m_debuglog += e.Message;
                }
            }
        }
    }
}
