using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Net.Http.Headers;

namespace WebPortal.Controllers
{
    public class UMEditorUploadController : Controller
    {
        private IHostingEnvironment _env;

        public UMEditorUploadController(IHostingEnvironment env)
        {
            _env = env;
        }
        //[Activate]
        //public IHostingEnvironment HostingEnvironment { get; set; }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Image()
        {
            //上传配置
            string pathbase = "upload/";                                                          //保存路径
            int size = 10;                     //文件大小限制,单位mb                                                                                   //文件大小限制，单位KB
            string[] filetype = { ".gif", ".png", ".jpg", ".jpeg", ".bmp" };                    //文件允许格式

            string callback = Context.Request.Query["callback"];
            string editorId = Context.Request.Query["editorid"];

            //上传图片
            Uploader up = new Uploader(_env,Request);
            var info =await up.UpFile(pathbase, filetype, size); //获取上传状态
            string json = BuildJson(info);

            Response.ContentType = "text/html";
            if (callback != null)
            {
                return Content(string.Format("<script>{0}(JSON.parse(\"{1}\"));</script>", callback, json));
            }
            else
            {
                return Content(json);
            }
        }

        private string BuildJson(Hashtable info)
        {
            List<string> fields = new List<string>();
            string[] keys = new string[] { "originalName", "name", "url", "size", "state", "type" };
            for (int i = 0; i < keys.Length; i++)
            {
                fields.Add(String.Format("\"{0}\": \"{1}\"", keys[i], info[keys[i]]));
            }
            return "{" + String.Join(",", fields) + "}";
        }
    }
}
