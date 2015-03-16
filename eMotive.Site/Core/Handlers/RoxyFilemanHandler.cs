using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;

namespace eMotive.SCE.Core.Handlers
{
    public class RoxyFilemanHandler : IHttpHandler, IRequiresSessionState
    {
        Dictionary<string, string> _settings;
        Dictionary<string, string> _lang;
        HttpResponse _r;
        HttpContext _context;
        private string ConfFile;// = "/Fileman/conf.json";

        public void ProcessRequest(HttpContext context)
        {
            _context = context;
            _r = context.Response;
            
            ConfFile = string.Format("{0}/Fileman/conf.json", VirtualPathUtility.ToAbsolute("~/"));


            var action = "DIRLIST";
            try
            {
                if (_context.Request["a"] != null)
                    action = _context.Request["a"];

                VerifyAction(action);
                switch (action.ToUpper())
                {
                    case "DIRLIST":
                        ListDirTree(_context.Request["type"]);
                        break;
                    case "FILESLIST":
                        ListFiles(_context.Request["d"], _context.Request["type"]);
                        break;
                    case "COPYDIR":
                        CopyDir(_context.Request["d"], _context.Request["n"]);
                        break;
                    case "COPYFILE":
                        CopyFile(_context.Request["f"], _context.Request["n"]);
                        break;
                    case "CREATEDIR":
                        CreateDir(_context.Request["d"], _context.Request["n"]);
                        break;
                    case "DELETEDIR":
                        DeleteDir(_context.Request["d"]);
                        break;
                    case "DELETEFILE":
                        DeleteFile(_context.Request["f"]);
                        break;
                    case "DOWNLOAD":
                        DownloadFile(_context.Request["f"]);
                        break;
                    case "DOWNLOADDIR":
                        DownloadDir(_context.Request["d"]);
                        break;
                    case "MOVEDIR":
                        MoveDir(_context.Request["d"], _context.Request["n"]);
                        break;
                    case "MOVEFILE":
                        MoveFile(_context.Request["f"], _context.Request["n"]);
                        break;
                    case "RENAMEDIR":
                        RenameDir(_context.Request["d"], _context.Request["n"]);
                        break;
                    case "RENAMEFILE":
                        RenameFile(_context.Request["f"], _context.Request["n"]);
                        break;
                    case "GENERATETHUMB":
                        int w = 140, h = 0;
                        int.TryParse(_context.Request["width"].Replace("px", ""), out w);
                        int.TryParse(_context.Request["height"].Replace("px", ""), out h);
                        ShowThumbnail(_context.Request["f"], w, h);
                        break;
                    case "UPLOAD":
                        Upload(_context.Request["d"]);
                        break;
                    default:
                        _r.Write(GetErrorRes("This action is not implemented."));
                        break;
                }

            }
            catch (Exception ex)
            {
                if (action == "UPLOAD")
                {
                    _r.Write("<script>");
                    _r.Write("parent.fileUploaded(" + GetErrorRes(LangRes("E_UploadNoFiles")) + ");");
                    _r.Write("</script>");
                }
                else
                {
                    _r.Write(GetErrorRes(ex.Message));
                }
            }

        }
        private string FixPath(string path)
        {
            if (!path.StartsWith("~"))
            {
                if (!path.StartsWith("/"))
                    path = "/" + path;
                path = "~" + path;
            }
            return _context.Server.MapPath(path);
        }

        private static string GetLangFile()
        {
            return "/Fileman/lang/en.json";
        }

        private string LangRes(string name)
        {
            var ret = name;
            if (_lang == null)
                _lang = ParseJSON(GetLangFile());
            if (_lang.ContainsKey(name))
                ret = _lang[name];

            return ret;
        }

        private static string GetFileType(string ext)
        {
            string ret = "file";
            ext = ext.ToLower();
            if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif")
                ret = "image";
            else if (ext == ".swf" || ext == ".flv")
                ret = "flash";
            return ret;
        }

        private bool CanHandleFile(string filename)
        {
            var ret = false;
            var file = new FileInfo(filename);
            var ext = file.Extension.Replace(".", "").ToLower();
            var setting = GetSetting("FORBIDDEN_UPLOADS").Trim().ToLower();
            if (setting != "")
            {
                var tmp = new ArrayList();
                tmp.AddRange(Regex.Split(setting, "\\s+"));
                if (!tmp.Contains(ext))
                    ret = true;
            }
            setting = GetSetting("ALLOWED_UPLOADS").Trim().ToLower();
            if (setting != "")
            {
                var tmp = new ArrayList();
                tmp.AddRange(Regex.Split(setting, "\\s+"));
                if (!tmp.Contains(ext))
                    ret = false;
            }

            return ret;
        }

        private Dictionary<string, string> ParseJSON(string file)
        {


            var ret = new Dictionary<string, string>();
            var json = string.Empty;
            try
            {
                json = File.ReadAllText(_context.Server.MapPath(file), System.Text.Encoding.UTF8);
            }
            catch (Exception ex) { }

            json = json.Trim();
            if (!string.IsNullOrEmpty(json))
            {
                if (json.StartsWith("{"))
                    json = json.Substring(1, json.Length - 2);
                json = json.Trim();
                json = json.Substring(1, json.Length - 2);
                var lines = Regex.Split(json, "\"\\s*,\\s*\"");

                foreach (var tmp in lines.Select(line => Regex.Split(line, "\"\\s*:\\s*\"")))
                {
                    try
                    {
                        if (tmp[0] != "" && !ret.ContainsKey(tmp[0]))
                        {
                            ret.Add(tmp[0], tmp[1]);
                        }
                    }
                    catch (Exception ex) { }
                }
            }
            return ret;
        }

        private string GetFilesRoot()
        {
            var ret = GetSetting("FILES_ROOT");
            if (GetSetting("SESSION_PATH_KEY") != "" && _context.Session[GetSetting("SESSION_PATH_KEY")] != null)
                ret = (string)_context.Session[GetSetting("SESSION_PATH_KEY")];

            ret = string.IsNullOrEmpty(ret) ? _context.Server.MapPath("/Uploads") : FixPath(ret);

            return ret;
        }

        private void LoadConf()
        {
            if (_settings == null)
                _settings = ParseJSON(ConfFile);
        }

        private string GetSetting(string name)
        {
            var ret = string.Empty;
            LoadConf();
            if (_settings.ContainsKey(name))
                ret = _settings[name];

            return ret;
        }
        private void CheckPath(string path)
        {
            if (FixPath(path).IndexOf(GetFilesRoot(), StringComparison.Ordinal) != 0)
            {
                throw new Exception("Access to " + path + " is denied");
            }
        }
        private void VerifyAction(string action)
        {
            var setting = GetSetting(action);
            if (setting.IndexOf("?", StringComparison.Ordinal) > -1)
                setting = setting.Substring(0, setting.IndexOf("?", StringComparison.Ordinal));
            if (!setting.StartsWith("/"))
                setting = "/" + setting;
            //setting = ".." + setting;

            if (_context.Server.MapPath(setting) != _context.Server.MapPath(_context.Request.Url.LocalPath))
                throw new Exception(LangRes("E_ActionDisabled"));
        }
        private static string GetResultStr(string type, string msg)
        {
            return "{\"res\":\"" + type + "\",\"msg\":\"" + msg.Replace("\"", "\\\"") + "\"}";
        }
        private static string GetSuccessRes(string msg = "")
        {
            return GetResultStr("ok", msg);
        }

        private static string GetErrorRes(string msg)
        {
            return GetResultStr("error", msg);
        }

        private static void _copyDir(string path, string dest)
        {
            if (!Directory.Exists(dest))
                Directory.CreateDirectory(dest);
            foreach (var f in Directory.GetFiles(path))
            {
                var file = new FileInfo(f);
                if (!File.Exists(Path.Combine(dest, file.Name)))
                {
                    File.Copy(f, Path.Combine(dest, file.Name));
                }
            }
            foreach (var d in Directory.GetDirectories(path))
            {
                var dir = new DirectoryInfo(d);
                _copyDir(d, Path.Combine(dest, dir.Name));
            }
        }
        private void CopyDir(string path, string newPath)
        {
            CheckPath(path);
            CheckPath(newPath);
            var dir = new DirectoryInfo(FixPath(path));
            var newDir = new DirectoryInfo(FixPath(newPath + "/" + dir.Name));

            if (!dir.Exists)
                throw new Exception(LangRes("E_CopyDirInvalidPath"));

            if (newDir.Exists)
                throw new Exception(LangRes("E_DirAlreadyExists"));

            _copyDir(dir.FullName, newDir.FullName);

            _r.Write(GetSuccessRes());
        }

        private static string MakeUniqueFilename(string dir, string filename)
        {
            var ret = filename;
            var i = 0;
            while (File.Exists(Path.Combine(dir, ret)))
            {
                i++;
                ret = Path.GetFileNameWithoutExtension(filename) + " - Copy " + i.ToString(CultureInfo.InvariantCulture) + Path.GetExtension(filename);
            }
            return ret;
        }

        private void CopyFile(string path, string newPath)
        {
            CheckPath(path);
            var file = new FileInfo(FixPath(path));
            newPath = FixPath(newPath);
            if (!file.Exists)
                throw new Exception(LangRes("E_CopyFileInvalisPath"));


            var newName = MakeUniqueFilename(newPath, file.Name);

            try
            {
                File.Copy(file.FullName, Path.Combine(newPath, newName));
                _r.Write(GetSuccessRes());
            }
            catch (Exception ex)
            {
                throw new Exception(LangRes("E_CopyFile"));
            }
        }

        private void CreateDir(string path, string name)
        {
            CheckPath(path);
            path = FixPath(path);

            if (!Directory.Exists(path))
                throw new Exception(LangRes("E_CreateDirInvalidPath"));

            try
            {
                path = Path.Combine(path, name);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                _r.Write(GetSuccessRes());
            }
            catch (Exception ex)
            {
                throw new Exception(LangRes("E_CreateDirFailed"));
            }
        }

        private void DeleteDir(string path)
        {
            CheckPath(path);
            path = FixPath(path);

            if (!Directory.Exists(path))
                throw new Exception(LangRes("E_DeleteDirInvalidPath"));

            if (path == GetFilesRoot())
                throw new Exception(LangRes("E_CannotDeleteRoot"));

            if (Directory.GetDirectories(path).Length > 0 || Directory.GetFiles(path).Length > 0)
                throw new Exception(LangRes("E_DeleteNonEmpty"));

            try
            {
                Directory.Delete(path);
                _r.Write(GetSuccessRes());
            }
            catch (Exception ex)
            {
                throw new Exception(LangRes("E_CannotDeleteDir"));
            }
        }

        private void DeleteFile(string path)
        {
            CheckPath(path);
            path = FixPath(path);
            if (!File.Exists(path))
                throw new Exception(LangRes("E_DeleteFileInvalidPath"));

            try
            {
                File.Delete(path);
                _r.Write(GetSuccessRes());
            }
            catch (Exception ex)
            {
                throw new Exception(LangRes("E_DeletеFile"));
            }
        }
        private List<string> GetFiles(string path, string type)
        {
            var ret = new List<string>();
            if (type == "#")
                type = "";
            var files = Directory.GetFiles(path);

            foreach (var f in files)
            {
                if ((GetFileType(new FileInfo(f).Extension) == type) || (type == ""))
                    ret.Add(f);
            }
            return ret;
        }

        private static ArrayList ListDirs(string path)
        {
            var dirs = Directory.GetDirectories(path);
            var ret = new ArrayList();

            foreach (var dir in dirs)
            {
                ret.Add(dir);
                ret.AddRange(ListDirs(dir));
            }
            return ret;
        }

        private void ListDirTree(string type)
        {
            var d = new DirectoryInfo(GetFilesRoot());
            if (!d.Exists)
                throw new Exception("Invalid files root directory. Check your configuration.");

            var dirs = ListDirs(d.FullName);
            dirs.Insert(0, d.FullName);

            var localPath = _context.Server.MapPath("~/");
            _r.Write("[");

            for (var i = 0; i < dirs.Count; i++)
            {
                var dir = (string)dirs[i];
                _r.Write("{\"p\":\"/" + dir.Replace(localPath, "").Replace("\\", "/") + "\",\"f\":\"" + GetFiles(dir, type).Count + "\",\"d\":\"" + Directory.GetDirectories(dir).Length.ToString(CultureInfo.InvariantCulture) + "\"}");
                if (i < dirs.Count - 1)
                    _r.Write(",");
            }
            _r.Write("]");
        }

        private static double LinuxTimestamp(DateTime d)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime();
            var timeSpan = (d.ToLocalTime() - epoch);

            return timeSpan.TotalSeconds;

        }

        private void ListFiles(string path, string type)
        {
            CheckPath(path);
            var fullPath = FixPath(path);
            var files = GetFiles(fullPath, type);
            _r.Write("[");
            for (var i = 0; i < files.Count; i++)
            {
                var f = new FileInfo(files[i]);
                int w = 0, h = 0;

                if (GetFileType(f.Extension) == "image")
                {
                    try
                    {
                        var fs = new FileStream(f.FullName, FileMode.Open);
                        var img = Image.FromStream(fs);
                        w = img.Width;
                        h = img.Height;
                        fs.Close();
                        fs.Dispose();
                        img.Dispose();
                    }
                    catch (Exception ex) { throw ex; }
                }
                _r.Write("{");
                _r.Write("\"p\":\"" + path + "/" + f.Name + "\"");
                _r.Write(",\"t\":\"" + Math.Ceiling(LinuxTimestamp(f.LastWriteTime)) + "\"");
                _r.Write(",\"s\":\"" + f.Length + "\"");
                _r.Write(",\"w\":\"" + w + "\"");
                _r.Write(",\"h\":\"" + h + "\"");
                _r.Write("}");
                if (i < files.Count - 1)
                    _r.Write(",");
            }
            _r.Write("]");
        }

        private void DownloadDir(string path)
        {
            path = FixPath(path);
            if (!Directory.Exists(path))
                throw new Exception(LangRes("E_CreateArchive"));
            var dirName = new FileInfo(path).Name;
            var tmpZip = _context.Server.MapPath("../tmp/" + dirName + ".zip");
            if (File.Exists(tmpZip))
                File.Delete(tmpZip);

            ZipFile.CreateFromDirectory(path, tmpZip, CompressionLevel.Fastest, true);
            _r.Clear();
            _r.Headers.Add("Content-Disposition", "attachment; filename=\"" + dirName + ".zip\"");
            _r.ContentType = "application/force-download";
            _r.TransmitFile(tmpZip);
            _r.Flush();
            File.Delete(tmpZip);
            _r.End();
        }
        private void DownloadFile(string path)
        {
            CheckPath(path);
            var file = new FileInfo(FixPath(path));

            if (!file.Exists) return;

            _r.Clear();
            _r.Headers.Add("Content-Disposition", "attachment; filename=\"" + file.Name + "\"");
            _r.ContentType = "application/force-download";
            _r.TransmitFile(file.FullName);
            _r.Flush();
            _r.End();
        }
        private void MoveDir(string path, string newPath)
        {
            CheckPath(path);
            CheckPath(newPath);
            var source = new DirectoryInfo(FixPath(path));
            var dest = new DirectoryInfo(FixPath(Path.Combine(newPath, source.Name)));
            if (dest.FullName.IndexOf(source.FullName, StringComparison.Ordinal) == 0)
                throw new Exception(LangRes("E_CannotMoveDirToChild"));

            if (!source.Exists)
                throw new Exception(LangRes("E_MoveDirInvalisPath"));

            if (dest.Exists)
                throw new Exception(LangRes("E_DirAlreadyExists"));


            try
            {
                source.MoveTo(dest.FullName);
                _r.Write(GetSuccessRes());
            }
            catch (Exception ex)
            {
                throw new Exception(LangRes("E_MoveDir") + " \"" + path + "\"");
            }


        }
        private void MoveFile(string path, string newPath)
        {
            CheckPath(path);
            CheckPath(newPath);
            var source = new FileInfo(FixPath(path));
            var dest = new FileInfo(FixPath(newPath));
            if (!source.Exists)
                throw new Exception(LangRes("E_MoveFileInvalisPath"));

            if (dest.Exists)
                throw new Exception(LangRes("E_MoveFileAlreadyExists"));

            try
            {
                source.MoveTo(dest.FullName);
                _r.Write(GetSuccessRes());
            }
            catch (Exception ex)
            {
                throw new Exception(LangRes("E_MoveFile") + " \"" + path + "\"");
            }

        }

        private void RenameDir(string path, string name)
        {
            CheckPath(path);
            var source = new DirectoryInfo(FixPath(path));
            var dest = new DirectoryInfo(Path.Combine(source.Parent.FullName, name));
            if (source.FullName == GetFilesRoot())
                throw new Exception(LangRes("E_CannotRenameRoot"));

            if (!source.Exists)
                throw new Exception(LangRes("E_RenameDirInvalidPath"));

            if (dest.Exists)
                throw new Exception(LangRes("E_DirAlreadyExists"));

            try
            {
                source.MoveTo(dest.FullName);
                _r.Write(GetSuccessRes());
            }
            catch (Exception ex)
            {
                throw new Exception(LangRes("E_RenameDir") + " \"" + path + "\"");
            }

        }

        private void RenameFile(string path, string name)
        {
            CheckPath(path);
            var source = new FileInfo(FixPath(path));
            var dest = new FileInfo(Path.Combine(source.Directory.FullName, name));
            if (!source.Exists)
                throw new Exception(LangRes("E_RenameFileInvalidPath"));

            if (!CanHandleFile(name))
                throw new Exception(LangRes("E_FileExtensionForbidden"));

            try
            {
                source.MoveTo(dest.FullName);
                _r.Write(GetSuccessRes());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "; " + LangRes("E_RenameFile") + " \"" + path + "\"");
            }

        }

        private static bool ThumbnailCallback()
        {
            return false;
        }

        private void ShowThumbnail(string path, int width, int height)
        {
            CheckPath(path);
            var fs = new FileStream(FixPath(path), FileMode.Open);
            var img = new Bitmap(Bitmap.FromStream(fs));
            fs.Close();
            fs.Dispose();
            int cropWidth = img.Width, cropHeight = img.Height;
            int cropX = 0, cropY = 0;

            double imgRatio = (double)img.Width / (double)img.Height;

            if (height == 0)
                height = Convert.ToInt32(Math.Floor((double)width / imgRatio));

            if (width > img.Width)
                width = img.Width;
            if (height > img.Height)
                height = img.Height;

            double cropRatio = (double)width / (double)height;
            cropWidth = Convert.ToInt32(Math.Floor((double)img.Height * cropRatio));
            cropHeight = Convert.ToInt32(Math.Floor((double)cropWidth / cropRatio));
            if (cropWidth > img.Width)
            {
                cropWidth = img.Width;
                cropHeight = Convert.ToInt32(Math.Floor((double)cropWidth / cropRatio));
            }
            if (cropHeight > img.Height)
            {
                cropHeight = img.Height;
                cropWidth = Convert.ToInt32(Math.Floor((double)cropHeight * cropRatio));
            }
            if (cropWidth < img.Width)
            {
                cropX = Convert.ToInt32(Math.Floor((double)(img.Width - cropWidth) / 2));
            }
            if (cropHeight < img.Height)
            {
                cropY = Convert.ToInt32(Math.Floor((double)(img.Height - cropHeight) / 2));
            }

            var area = new Rectangle(cropX, cropY, cropWidth, cropHeight);
            var cropImg = img.Clone(area, PixelFormat.DontCare);
            img.Dispose();
            var imgCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);

            _r.AddHeader("Content-Type", "image/png");
            cropImg.GetThumbnailImage(width, height, imgCallback, IntPtr.Zero).Save(_r.OutputStream, ImageFormat.Png);
            _r.OutputStream.Close();
            cropImg.Dispose();
        }

        private static ImageFormat GetImageFormat(string filename)
        {
            var ret = ImageFormat.Jpeg;
            switch (new FileInfo(filename).Extension.ToLower())
            {
                case ".png": ret = ImageFormat.Png; break;
                case ".gif": ret = ImageFormat.Gif; break;
                case ".jpg": ret = ImageFormat.Jpeg; break;
                case ".jpeg": ret = ImageFormat.Jpeg; break;
            }
            return ret;
        }

        private void ImageResize(string path, string dest, int width, int height)
        {
            var fs = new FileStream(path, FileMode.Open);
            var img = Image.FromStream(fs);
            fs.Close();
            fs.Dispose();
            float ratio = (float)img.Width / (float)img.Height;
            if ((img.Width <= width && img.Height <= height) || (width == 0 && height == 0))
                return;

            int newWidth = width;
            int newHeight = Convert.ToInt16(Math.Floor((float)newWidth / ratio));
            if ((height > 0 && newHeight > height) || (width == 0))
            {
                newHeight = height;
                newWidth = Convert.ToInt16(Math.Floor((float)newHeight * ratio));
            }
            var newImg = new Bitmap(newWidth, newHeight);
            var g = Graphics.FromImage((Image)newImg);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(img, 0, 0, newWidth, newHeight);
            img.Dispose();
            g.Dispose();
            if (string.IsNullOrEmpty(dest))
            {
                newImg.Save(dest, GetImageFormat(dest));
            }
            newImg.Dispose();
        }

        private void Upload(string path)
        {
            CheckPath(path);
            path = FixPath(path);
            var res = GetSuccessRes();
            try
            {
                for (var i = 0; i < HttpContext.Current.Request.Files.Count; i++)
                {
                    if (CanHandleFile(HttpContext.Current.Request.Files[i].FileName))
                    {
                        var filename = MakeUniqueFilename(path, HttpContext.Current.Request.Files[i].FileName);
                        var dest = Path.Combine(path, filename);
                        HttpContext.Current.Request.Files[i].SaveAs(dest);
                        if (GetFileType(new FileInfo(filename).Extension) == "image")
                        {
                            int w = 0;
                            int h = 0;
                            int.TryParse(GetSetting("MAX_IMAGE_WIDTH"), out w);
                            int.TryParse(GetSetting("MAX_IMAGE_HEIGHT"), out h);
                            ImageResize(dest, dest, w, h);
                        }
                    }
                    else
                        res = GetSuccessRes(LangRes("E_UploadNotAll"));
                }
            }
            catch (Exception ex)
            {
                res = GetErrorRes(ex.Message);
            }
            _r.Write("<script>");
            _r.Write("parent.fileUploaded(" + res + ");");
            _r.Write("</script>");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

    }
}