using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileReader
{
    /// <summary>
    /// Try to read file to a string.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string ReadString(string path)
    {
        Debug.Log("Trying to read " + path);
        if (File.Exists(path))
        {
            // Read the file if it exists
            StreamReader reader = new StreamReader(path);
            string fileText = reader.ReadToEnd();
            reader.Close();
            Debug.Log("File read succesully!");
            return fileText;
        }
        else
        {
            throw (new FileNotFoundException("File not found: '" + path + "'."));
        }
    }
}
