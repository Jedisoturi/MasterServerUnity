using System;
using System.Collections;
using System.Collections.Generic;
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
    public static readonly Guid appId = Guid.Parse("b6a7ab1d-1d6f-4ee4-a32c-1eeed3eed8ee");
    public const string appIdHeader = "AppId";
}