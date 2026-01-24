using System;
using System.Diagnostics;
using System.IO;
using System.Text;
namespace TestRF64

{
public class Rf64StreamingReader : IDisposable
{
    private readonly FileStream _fs;
    private readonly BinaryReader _br;

    public WaveFormat Format { get; private set; }
    public long DataOffset { get; private set; }
    public long DataSize { get; private set; }

    public Stream DataStream => _fs;

    public Rf64StreamingReader(string path)
    {
        _fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        _br = new BinaryReader(_fs);

        ParseHeader();
    }

    private void ParseHeader()
    {
        string riffId = Encoding.ASCII.GetString(_br.ReadBytes(4));
        if (riffId != "RF64" && riffId != "RIFF")
            throw new InvalidDataException("Not a RF64/WAV file");

        uint riffSize = _br.ReadUInt32();
        string waveId = Encoding.ASCII.GetString(_br.ReadBytes(4));
        if (waveId != "WAVE")
            throw new InvalidDataException("Not a WAVE file");

        long? rf64DataSize = null;

        while (_fs.Position < _fs.Length)
        {
            if (!TryReadChunkHeader(out string chunkId, out uint chunkSize))
                break;

            long chunkStart = _fs.Position;

            switch (chunkId)
            {
                case "ds64":

                    long riff64 = _br.ReadInt64();
                    rf64DataSize = _br.ReadInt64();
                    long sampleCount = _br.ReadInt64();
                    break;

                case "fmt ":
                    Format = ReadFmtChunk();
                    break;

                case "data":
                    DataOffset = _fs.Position;
                    DataSize = (chunkSize == 0xFFFFFFFF && rf64DataSize.HasValue)
                        ? rf64DataSize.Value
                        : chunkSize;
                   return;
            }
            long endPos = chunkStart + chunkSize;
            _fs.Position = (endPos + 1) & ~1;
        }

        throw new InvalidDataException("No data chunk found");
    }

    private bool TryReadChunkHeader(out string id, out uint size)
    {
        id = null;
        size = 0;

        if (_fs.Position + 8 > _fs.Length)
            return false;

        id = Encoding.ASCII.GetString(_br.ReadBytes(4));
        size = _br.ReadUInt32();
        return true;
    }

    private WaveFormat ReadFmtChunk()
    {
        var fmt = new WaveFormat();
        fmt.AudioFormat = _br.ReadUInt16();
        fmt.NumChannels = _br.ReadUInt16();
        fmt.SampleRate = _br.ReadUInt32();
        fmt.ByteRate = _br.ReadUInt32();
        fmt.BlockAlign = _br.ReadUInt16();
        fmt.BitsPerSample = _br.ReadUInt16();
        return fmt;
    }

    public int ReadSamples(byte[] buffer, int offset, int count)
    {
        long remaining = (DataOffset + DataSize) - _fs.Position;
        if (remaining <= 0)
            return 0;

        int toRead = (int)Math.Min(count, remaining);
        return _fs.Read(buffer, offset, toRead);
    }

    public void Dispose()
    {
        _br?.Dispose();
        _fs?.Dispose();
    }
}

public class WaveFormat
{
    public ushort AudioFormat;
    public ushort NumChannels;
    public uint SampleRate;
    public uint ByteRate;
    public ushort BlockAlign;
    public ushort BitsPerSample;
}
}
