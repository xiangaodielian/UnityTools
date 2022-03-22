/***************************************************************
* Author: WuWenhua
* Create: 2022/3/22 16:28:18
* Note  : 外部创建的文本文件由于导入过来，会被重载并生成mate文件，编码格式会被修改为ANSI
***************************************************************/

using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ProcessClassImport: AssetPostprocessor
{
    /// <summary>
    /// 所有的资源的导入，删除，移动，都会调用此方法，注意，这个方法是static的
    /// </summary>
    /// <param name="importedAsset">导入的资源队列</param>
    /// <param name="deletedAssets">删除的资源队列</param>
    /// <param name="movedAssets">移动的资源队列</param>
    /// <param name="movedFromAssetPaths">自某个目录移动路径队列</param>
    public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        //Debug.Log("OnPostprocessAllAssets");
        foreach (string str in importedAsset)
        {
            if (IsFileOfType(str, ".cs") && GetEncoding(@str) != Encoding.UTF8)
            {
                var s = File.ReadAllText(str, Encoding.Default);
                try
                {
                    File.WriteAllText(str, s, Encoding.UTF8);
                }
                catch (Exception e)
                {
                    continue;
                }
            }
        }
    }

    /// <summary>
    /// 扩展名
    /// </summary>
    /// <param name="filepath"></param>
    /// <param name="typeExtenstion"></param>
    /// <returns></returns>
    public static bool IsFileOfType(string filepath, string typeExtenstion)
    {
        if (string.IsNullOrEmpty(filepath))
        {
            return false;
        }
        return filepath.ToLower().EndsWith(typeExtenstion.ToLower());
    }

    /// <summary>
    /// 取得一个文本文件的编码方式。如果无法在文件头部找到有效的前导符，Encoding.Default将被返回。
    /// </summary>
    /// <param name="fileName">文件名。</param>
    /// <returns></returns>
    public static Encoding GetEncoding(string fileName)
    {
        return GetEncoding(fileName, Encoding.Default);
    }
    /// <summary>
    /// 取得一个文本文件流的编码方式。
    /// </summary>
    /// <param name="stream">文本文件流。</param>
    /// <returns></returns>

    public static Encoding GetEncoding(FileStream stream)
    {
        return GetEncoding(stream, Encoding.Default);
    }
    /// <summary>
    /// 取得一个文本文件的编码方式。
    /// </summary>
    /// <param name="fileName">文件名。</param>
    /// <param name="defaultEncoding">默认编码方式。当该方法无法从文件的头部取得有效的前导符时，将返回该编码方式。</param>
    /// <returns></returns>
    public static Encoding GetEncoding(string fileName, Encoding defaultEncoding)
    {
        FileStream fs = null;
        Encoding targetEncoding = defaultEncoding;
        try
        {
            fs = new FileStream(fileName, FileMode.Open);
            targetEncoding = GetEncoding(fs, defaultEncoding);
        }
        catch
        {
        }
        if (fs != null)
        {
            fs.Close();
        }
        return targetEncoding;
    }

    /// <summary>
    /// 取得一个文本文件流的编码方式。
    /// </summary>
    /// <param name="stream">文本文件流。</param>
    /// <param name="defaultEncoding">默认编码方式。当该方法无法从文件的头部取得有效的前导符时，将返回该编码方式。</param>
    /// <returns></returns>
    public static Encoding GetEncoding(FileStream stream, Encoding defaultEncoding)
    {
        Encoding targetEncoding = defaultEncoding;
        if (stream != null && stream.Length >= 2)
        {
            //保存文件流的前4个字节
            byte byte1 = 0;
            byte byte2 = 0;
            byte byte3 = 0;
            byte byte4 = 0;
            //保存当前Seek位置
            long origPos = stream.Seek(0, SeekOrigin.Begin);
            stream.Seek(0, SeekOrigin.Begin);
            int nByte = stream.ReadByte();
            byte1 = Convert.ToByte(nByte);
            byte2 = Convert.ToByte(stream.ReadByte());
            if (stream.Length >= 3)
            {
                byte3 = Convert.ToByte(stream.ReadByte());
            }
            if (stream.Length >= 4)
            {
                byte4 = Convert.ToByte(stream.ReadByte());
            }
            //根据文件流的前4个字节判断Encoding
            //Unicode {0xFF, 0xFE};
            //BE-Unicode {0xFE, 0xFF};
            //UTF8 = {0xEF, 0xBB, 0xBF};
            if (byte1 == 0xFE && byte2 == 0xFF)//UnicodeBe
            {
                targetEncoding = Encoding.BigEndianUnicode;
            }
            else if (byte1 == 0xFF && byte2 == 0xFE && byte3 != 0xFF)//Unicode
            {
                targetEncoding = Encoding.Unicode;
            }
            else if (byte1 == 0xEF && byte2 == 0xBB && byte3 == 0xBF)//UTF8
            {
                targetEncoding = Encoding.UTF8;
            }
            else if (byte2 == 0x0)//Unicode
            {
                targetEncoding = Encoding.Unicode;
            }
            //恢复Seek位置　　　
            stream.Seek(origPos, SeekOrigin.Begin);
        }
        return targetEncoding;
    }
}
