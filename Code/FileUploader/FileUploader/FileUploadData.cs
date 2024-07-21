using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class FileUploadData
{

    private string _localPath = "";
    private string _remotePath = "";
    public void SetLocalPath(string path)
    {
        _localPath = path;
    }

    public string GetLocalPath()
    {
        return _localPath;
    }

    public void SetRemotePath(string path)
    {
        _remotePath = path;
    }

    public string GetContainerName()
    {
        if (string.IsNullOrEmpty(_remotePath)) return "";
        var firstSlashIndex = _remotePath.IndexOf("/");
        if (firstSlashIndex <= 0) return "";

        return _remotePath.Substring(0, firstSlashIndex).ToLower();
    }

    public string GetBlobPath()
    {
        var containerName = GetContainerName();
        if (_remotePath != null && _remotePath.Length > containerName.Length)
        {
            return _remotePath.Substring(containerName.Length + 1);
        }
        return "";
    }

    public bool HasValidData()
    {
        return !string.IsNullOrEmpty(GetLocalPath()) &&
            !string.IsNullOrEmpty(GetContainerName()) &&
            !string.IsNullOrEmpty(GetBlobPath());
    }

}
