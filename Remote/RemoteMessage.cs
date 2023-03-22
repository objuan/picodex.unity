using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Text;


public enum RemoteMessageType
{
    LOGIN = 0,
    DISCONNECT,
    SCREEN,
    ACCELERATION,
    ATTITUDE,
    TOUCH,
}

public class RemoteMessage 
{
    static System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");

    public RemoteMessageType messageType;
    public string value;


    public RemoteMessage(RemoteMessageType messageType, Touch value)
    {
        this.messageType = messageType;
        this.value = string.Format(ci, "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}", value.fingerId, value.position.x, value.position.y,value.tapCount,(int)value.phase, value.pressure,(int)value.type,
            value.altitudeAngle, value.azimuthAngle, value.radius,value.radiusVariance);
    }

    public RemoteMessage(RemoteMessageType messageType, string value)
    {
        this.messageType = messageType;
        this.value = value;
    }
    public RemoteMessage(RemoteMessageType messageType, Vector3 value)
    {
        this.messageType = messageType;
        this.value = string.Format(ci,"{0} {1} {2}", value.x, value.y, value.z);
    }
    public RemoteMessage(RemoteMessageType messageType, Quaternion value)
    {
        this.messageType = messageType;
        this.value = string.Format(ci, "{0} {1} {2} {3}", value.x, value.y, value.z, value.w);
    }
    public RemoteMessage(RemoteMessageType messageType, params float[] values)
    {
        this.messageType = messageType;
        this.value = "";
        foreach(var v in values)
            value+= string.Format(ci, "{0} ", v);
    }

    public float GetFloat(int idx)
    {
        return Convert.ToSingle(value.Split(' ')[idx]);
    }
    public Vector3 GetVector( )
    {
        var s = value.Split(' ');

        return new Vector3( Convert.ToSingle(s[0]), Convert.ToSingle(s[1]), Convert.ToSingle(s[2]));
    }
    public Quaternion GetQuaternionr( )
    {
        var s = value.Split(' ');

        return new Quaternion(Convert.ToSingle(s[0]), Convert.ToSingle(s[1]), Convert.ToSingle(s[2]), Convert.ToSingle(s[3]));
    }
    public Touch GetTouch( )
    {
        var s = value.Split(' ');

        return new Touch()
        {
            fingerId = Convert.ToInt32(s[0]),
            position = new Vector2(Convert.ToSingle(s[1]), Convert.ToSingle(s[2])),
            tapCount = Convert.ToInt32(s[3]),
            phase = (TouchPhase)Convert.ToInt32(s[4]),
            pressure = Convert.ToSingle(s[5]),
            type = (TouchType)Convert.ToInt32(s[6]),
            altitudeAngle = Convert.ToSingle(s[7]),
            azimuthAngle = Convert.ToSingle(s[8]),
            radius = Convert.ToSingle(s[9]),
            radiusVariance = Convert.ToSingle(s[10]),
        };
    }

    public static RemoteMessage Parse(string msg)
    {
        //string[] ss = msg.Split(' ');
        int idx = msg.IndexOf( " ");
        return new RemoteMessage((RemoteMessageType)Convert.ToInt32(msg.Substring(0,idx)), msg.Substring(idx+1));
    }

    public string Serialize()
    {
        return string.Format("{0} {1}", (int)messageType, value);
    }
}