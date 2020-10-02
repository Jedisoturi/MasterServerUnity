using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

/// <summary>Sent from server to client.</summary>
public enum ServerPackets
{
    welcome = 1,
    spawnPlayer,
    playerPosition,
    playerRotation,
    playerDisconnected,
    playerHealth,
    playerRespawned,
    createItemSpawner,
    itemSpawned,
    itemPickedUp,
    spawnProjectile,
    projectilePosition,
    projectileExploded,
    udpTest,
    ack
}

/// <summary>Sent from client to server.</summary>
public enum ClientPackets
{
    welcomeReceived = 1,
    playerMovement,
    playerShoot,
    playerThrowItem,
    udpTestReceived,
}

public enum TransportType
{
    udp = 1,
    rudp
}

public class Packet : IDisposable
{
    private static class Constants
    {
        public const int byteLengthInBytes = 1;
        public const int shortLengthInBytes = 2;
        public const int intLengthInBytes = 4;
        public const int longLengthInBytes = 8;
        public const int floatLengthInBytes = 4;
        public const int boolLengthInBytes = 1;
    }

    private List<byte> _buffer;
    private byte[] _readableBuffer;
    private int _readPos;

    /// <summary>Creates a new empty packet (without an ID).</summary>
    public Packet()
    {
        _buffer = new List<byte>();
        _readPos = 0;
    }

    /// <summary>Creates a new packet with a given ID. Used for sending.</summary>
    /// <param name="id">The packet ID.</param>
    public Packet(int id) : this()
    {
        Write(id);
    }

    /// <summary>Creates a packet from which data can be read. Used for receiving.</summary>
    /// <param name="data">The bytes to add to the packet.</param>
    public Packet(byte[] data) : this()
    {
        SetBytes(data);
    }

    #region Functions
    /// <summary>Sets the packet's content and prepares it to be read.</summary>
    /// <param name="data">The bytes to add to the packet.</param>
    public void SetBytes(byte[] data)
    {
        Write(data);
        _readableBuffer = _buffer.ToArray();
    }

    /// <summary>Inserts the length of the packet's content at the start of the buffer.</summary>
    public void WriteLength()
    {
        _buffer.InsertRange(0, BitConverter.GetBytes(_buffer.Count));
    }

    /// <summary>Inserts the given int at the start of the buffer.</summary>
    /// <param name="value">The int to insert.</param>
    public void InsertInt(int value)
    {
        _buffer.InsertRange(0, BitConverter.GetBytes(value));
    }

    /// <summary>Gets the packet's content in array form.</summary>
    public byte[] ToArray()
    {
        _readableBuffer = _buffer.ToArray();
        return _readableBuffer;
    }

    /// <summary>Gets the length of the packet's content.</summary>
    public int Length()
    {
        return _buffer.Count;
    }

    /// <summary>Gets the length of the unread data contained in the packet.</summary>
    public int UnreadLength()
    {
        return Length() - _readPos;
    }

    /// <summary>Resets the packet instance to allow it to be reused.</summary>
    /// <param name="shouldReset">Whether or not to reset the packet.</param>
    public void Reset(bool shouldReset = true)
    {
        if (shouldReset)
        {
            _buffer.Clear();
            _readableBuffer = null;
            _readPos = 0;
        }
        else
        {
            _readPos -= Constants.intLengthInBytes;  // "Unread" the last read int
        }
    }
    #endregion

    #region Write Data
    /// <summary>Adds a byte to the packet.</summary>
    /// <param name="value">The byte to add.</param>
    public void Write(byte value)
    {
        _buffer.Add(value);
    }
    /// <summary>Adds an array of bytes to the packet.</summary>
    /// <param name="value">The byte array to add.</param>
    public void Write(byte[] value)
    {
        _buffer.AddRange(value);
    }
    /// <summary>Adds a short to the packet.</summary>
    /// <param name="value">The short to add.</param>
    public void Write(short value)
    {
        _buffer.AddRange(BitConverter.GetBytes(value));
    }
    /// <summary>Adds an int to the packet.</summary>
    /// <param name="value">The int to add.</param>
    public void Write(int value)
    {
        _buffer.AddRange(BitConverter.GetBytes(value));
    }
    /// <summary>Adds a long to the packet.</summary>
    /// <param name="value">The long to add.</param>
    public void Write(long value)
    {
        _buffer.AddRange(BitConverter.GetBytes(value));
    }
    /// <summary>Adds a float to the packet.</summary>
    /// <param name="value">The float to add.</param>
    public void Write(float value)
    {
        _buffer.AddRange(BitConverter.GetBytes(value));
    }
    /// <summary>Adds a bool to the packet.</summary>
    /// <param name="value">The bool to add.</param>
    public void Write(bool value)
    {
        _buffer.AddRange(BitConverter.GetBytes(value));
    }
    /// <summary>Adds a string to the packet.</summary>
    /// <param name="value">The string to add.</param>
    public void Write(string value)
    {
        Write(value.Length);  // Add the length of the string to the packet
        _buffer.AddRange(Encoding.ASCII.GetBytes(value));
    }

    /// <summary>Adds a Vector3 to the packet.</summary>
    /// <param name="value">The Vector3 to add.</param>
    public void Write(Vector3 value)
    {
        Write(value.x);
        Write(value.y);
        Write(value.z);
    }

    /// <summary>Adds a Quaternion to the packet.</summary>
    /// <param name="value">The Quaternion to add.</param>
    public void Write(Quaternion value)
    {
        Write(value.x);
        Write(value.y);
        Write(value.z);
        Write(value.w);
    }
    #endregion

    #region Read Data
    /// <summary>Reads a byte from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public byte ReadByte(bool moveReadPos = true)
    {
        if (_buffer.Count > _readPos)
        {
            // If there are unread bytes, read the current byte and return it
            byte value = _readableBuffer[_readPos];
            if (moveReadPos) _readPos += Constants.byteLengthInBytes;
            return value;
        }
        else
        {
            throw new Exception("Could not read value of type 'byte'!");
        }
    }

    /// <summary>Reads an array of bytes from the packet.</summary>
    /// <param name="length">The length of the byte array.</param>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public byte[] ReadBytes(int length, bool moveReadPos = true)
    {
        if (_buffer.Count > _readPos)
        {
            // If there are unread bytes, read bytes according to length and return them
            byte[] value = _buffer.GetRange(_readPos, length).ToArray();
            if (moveReadPos) _readPos += length;
            return value;
        }
        else
        {
            throw new Exception("Could not read value of type 'byte[]'!");
        }
    }

    /// <summary>Reads a short from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public short ReadShort(bool moveReadPos = true)
    {
        if (_buffer.Count > _readPos)
        {
            short value = BitConverter.ToInt16(_readableBuffer, _readPos);
            if (moveReadPos) _readPos += Constants.shortLengthInBytes;
            return value;
        }
        else
        {
            throw new Exception("Could not read value of type 'short'!");
        }
    }

    /// <summary>Reads an int from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public int ReadInt(bool moveReadPos = true)
    {
        if (_buffer.Count > _readPos)
        {
            int value = BitConverter.ToInt32(_readableBuffer, _readPos);
            if (moveReadPos) _readPos += Constants.intLengthInBytes;
            return value;
        }
        else
        {
            throw new Exception("Could not read value of type 'int'!");
        }
    }

    /// <summary>Reads a long from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public long ReadLong(bool moveReadPos = true)
    {
        if (_buffer.Count > _readPos)
        {
            long value = BitConverter.ToInt64(_readableBuffer, _readPos);
            if (moveReadPos) _readPos += Constants.longLengthInBytes;
            return value;
        }
        else
        {
            throw new Exception("Could not read value of type 'long'!");
        }
    }

    /// <summary>Reads a float from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public float ReadFloat(bool moveReadPos = true)
    {
        if (_buffer.Count > _readPos)
        {
            float value = BitConverter.ToSingle(_readableBuffer, _readPos);
            if (moveReadPos) _readPos += Constants.floatLengthInBytes;
            return value;
        }
        else
        {
            throw new Exception("Could not read value of type 'float'!");
        }
    }

    /// <summary>Reads a bool from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public bool ReadBool(bool moveReadPos = true)
    {
        if (_buffer.Count > _readPos)
        {
            bool value = BitConverter.ToBoolean(_readableBuffer, _readPos);
            if (moveReadPos) _readPos += Constants.boolLengthInBytes;
            return value;
        }
        else
        {
            throw new Exception("Could not read value of type 'bool'!");
        }
    }

    /// <summary>Reads a string from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public string ReadString(bool moveReadPos = true)
    {
        try
        {
            int length = ReadInt();
            string value = Encoding.ASCII.GetString(_readableBuffer, _readPos, length);
            if (moveReadPos && value.Length > 0) _readPos += length;
            return value;
        }
        catch
        {
            throw new Exception("Could not read value of type 'string'!");
        }
    }

    /// <summary>Reads a Vector3 from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public Vector3 ReadVector3(bool moveReadPos = true)
    {
        return new Vector3(ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos));
    }

    /// <summary>Reads a Quaternion from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public Quaternion ReadQuaternion(bool moveReadPos = true)
    {
        return new Quaternion(ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos));
    }
    #endregion

    private bool _disposed = false;

    /// <summary>Disposes the packet.</summary>
    /// <param name="clear">Whether or not to clear the variables of the packet</param>
    protected virtual void Dispose(bool clear)
    {
        if (_disposed) return;
        if (clear)
        {
            _buffer = null;
            _readableBuffer = null;
            _readPos = 0;
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
