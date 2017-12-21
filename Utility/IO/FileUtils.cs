using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.IO;

namespace insp.Utility.IO
{
    public static class FileUtils
    {
        #region XML File
        public static T XmlFileRead<T>(String filename,Encoding encode,bool throwException=false)
        {
            if (!File.Exists(filename))
                return default(T);
            FileStream fs = null;
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                return (T)xs.Deserialize(fs);
            }catch(Exception e)
            {
                if (throwException) throw e;
                return default(T);
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }

        public static void XmlFileWrite<T>(T obj,String filename, Encoding encode, bool throwException = false)
        {
            if (obj == null) return;
            FileStream fs = null;
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
                xs.Serialize(fs,obj);
            }
            catch (Exception e)
            {
                if (throwException) throw e;                
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }
        #endregion

        #region 路径
        /// <summary>
        /// 如果pathName无效，则返回当前执行程序的路径
        /// 如果pathName本身是完整路径，则直接返回pathName
        /// 如果pathName本身是相对路径，则返回相对路径相对于当前执行程序的完整路径
        /// 返回的路径必定以\\结尾
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        public static String GetDirectory(String pathName="")
        {
            if(pathName == null || pathName == "")
                return System.AppDomain.CurrentDomain.BaseDirectory;
            if (pathName.ToLower().Contains(":\\"))//已经是完整路径
            {
                if (!pathName.EndsWith("\\"))
                    return pathName + "\\";
                return pathName;
            }
            if (!pathName.EndsWith("\\"))
                pathName += "\\";
            return System.AppDomain.CurrentDomain.BaseDirectory + "\\" + pathName;
        }
        #endregion

        #region 文件读写
        /// <summary>
        /// 读文件，文件不存在则返回 “”
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static String ReadText(String filename)
        {
            if (!File.Exists(filename))
                return "";
            return File.ReadAllText(filename);
        }

        /// <summary>
        /// 读文件，文件不存在则返回 “”
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static List<String> ReadLines(String filename)
        {
            if (!File.Exists(filename))
                return new List<string>();
            return new List<string>(File.ReadAllLines(filename));
        }
        #endregion
    }
}
