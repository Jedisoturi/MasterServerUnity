using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Constants
{
    public const int ticksPerSec = 64;
    public const int msPerTick = 1000 / ticksPerSec;

    public const int byteLengthInBytes = 1;
    public const int shortLengthInBytes = 2;
    public const int intLengthInBytes = 4;
    public const int longLengthInBytes = 8;
    public const int floatLengthInBytes = 4;
    public const int boolLengthInBytes = 1;

    public const string apiAddress = "http://localhost:5000/";
    public static readonly byte[] secret = Encoding.UTF8.GetBytes("8EB6F8D11A6D3F42813B2C43DD6C8".ToLower()); 
}