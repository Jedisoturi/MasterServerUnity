using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Constants
{
    public const string apiAddress = "http://localhost:5000/";
    public static readonly byte[] secret = Encoding.UTF8.GetBytes("8EB6F8D11A6D3F42813B2C43DD6C8".ToLower());
}
