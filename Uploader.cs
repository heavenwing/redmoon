using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Collections;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Net.Http.Headers;

namespace WebPortal.Controllers
{
    /// <summary>
    /// UEditor编辑器通用上传类
    /// </summary>
    public class Uploader
    {
        IHostingEnvironment _env = null;
        Microsoft.AspNet.Http.HttpRequest _request;

        public Uploader(IHostingEnvironment _env, Microsoft.AspNet.Http.HttpRequest request)
        {
            this._env = _env;
            this._request = request;
        }

        string state = "SUCCESS";

        string URL = null;
        string currentType = null;
        string uploadpath = null;
        string filename = null;
        string originalName = null;
        ContentDispositionHeaderValue parsedContentDisposition = null;
        IFormFile uploadFile = null;

        public async System.Threading.Tasks.Task<Hashtable> UpFile(string pathbase, string[] filetype, int size)
        {
            pathbase = pathbase + DateTime.Now.ToString("yyyy-MM-dd") + "/";
            uploadpath = _env.MapPath(pathbase);//获取文件上传路径

            try
            {
                uploadFile = _request.Form.Files[0];
                parsedContentDisposition = ContentDispositionHeaderValue.Parse(uploadFile.ContentDisposition);
                originalName = parsedContentDisposition.FileName.Replace("\"", "");

                //目录创建
                createFolder();

                //格式验证
                if (checkType(filetype))
                {
                    state = "不允许的文件类型";
                }
                //大小验证
                if (checkSize(size))
                {
                    state = "文件大小超出网站限制";
                }
                //保存图片
                if (state == "SUCCESS")
                {
                    filename = reName();
                    await uploadFile.SaveAsAsync(uploadpath + filename);
                    URL = pathbase + filename;
                }
            }
            catch (Exception e)
            {
                state = "未知错误";
                URL = "";
            }
            return getUploadInfo();
        }

        /**
         * 获取上传信息
         * @return Hashtable
         */
        private Hashtable getUploadInfo()
        {
            Hashtable infoList = new Hashtable();

            infoList.Add("state", state);
            infoList.Add("url", URL);
            infoList.Add("originalName", originalName);
            infoList.Add("name", Path.GetFileName(URL));
            infoList.Add("size", uploadFile.Length);
            infoList.Add("type", Path.GetExtension(originalName));

            return infoList;
        }

        /**
         * 重命名文件
         * @return string
         */
        private string reName()
        {
            return Guid.NewGuid() + currentType;
        }

        /**
         * 文件类型检测
         * @return bool
         */
        private bool checkType(string[] filetype)
        {
            currentType = Path.GetExtension(originalName);
            return Array.IndexOf(filetype, currentType) == -1;
        }

        /**
         * 文件大小检测
         * @param int
         * @return bool
         */
        private bool checkSize(int size)
        {
            return uploadFile.Length >= (size * 1024 * 1024);
        }

        /**
         * 按照日期自动创建存储文件夹
         */
        private void createFolder()
        {
            if (!Directory.Exists(uploadpath))
            {
                Directory.CreateDirectory(uploadpath);
            }
        }

        /**
         * 删除存储文件夹
         * @param string
         */
        public void deleteFolder(string path)
        {
            //if (Directory.Exists(path))
            //{
            //    Directory.Delete(path, true);
            //}
        }
    }
}

