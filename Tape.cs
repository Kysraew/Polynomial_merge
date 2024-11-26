using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polynomial_merge
{
    public class Tape
    {

        public string FileName { get; private set; }
        public bool IsActive { get; private set; }
        private FileStream _stream;
        private readonly int _bufferSize;
        public readonly int MaxRecordLength;
        private byte[] _buffer;
        private int _bufferIndex;
        private int _bufferOccupancy;
        private bool _readMode;
        //private byte[] previousBuffer; 
        public int DiscReadsCounter { get; private set; }
        public int DiscWritesCounter { get; private set; }
        public int RecordReadsCounter { get; private set; }
        public int RecordWritesCounter { get; private set; }

        public Tape(string fileName)
        {
            _bufferSize = GlobalConfig.PageSize;
            MaxRecordLength = GlobalConfig.MaxRecordLength;

            _buffer = new byte[_bufferSize];
            _bufferIndex = 0;
            FileName = fileName;
            _stream = new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite);

            _readMode = false;
            _bufferOccupancy = 0;

            IsActive = true;

            DiscReadsCounter = 0;
        }

        public void DeleteTape()
        {
            _stream.Close();
            File.Delete(FileName);
        }
        public void ChangeFileName(string newFileName)
        {
            long streamIndex = _stream.Position;
            
            _stream.Close();
            File.Move(FileName, newFileName);
            FileName = newFileName;
            _stream = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite);
            _stream.Position = streamIndex;
        }
        private void CheckActive()
        {
            if (!IsActive)
            {
                throw new Exception("Tape is not active!");
            }
        }
        public char[]? ReadRecord()
        {
            CheckActive();
            SetReadMode();
            if (_bufferOccupancy == 0)
            {
                _bufferOccupancy = _stream.Read(_buffer);
                DiscReadsCounter++;

                if (_bufferOccupancy == 0)
                {
                    return null;
                }
            }

            int recordBufferLengh = 0;
            byte[] recordBuffer = new byte[MaxRecordLength];
            char[] testBuffer = Encoding.ASCII.GetChars(_buffer);
            while (recordBufferLengh < MaxRecordLength)
            {
                if (_bufferIndex  >= _bufferOccupancy)
                {
                    //previousBuffer = _buffer;
                    _bufferOccupancy = _stream.Read(_buffer);
                    DiscReadsCounter++;

                    _bufferIndex = 0;

                    if (_bufferOccupancy == 0)
                    {
                        return null;
                    }

                }

                recordBuffer[recordBufferLengh] = _buffer[_bufferIndex];
                recordBufferLengh++;
                _bufferIndex++;

                if (recordBuffer[recordBufferLengh - 1] == '\0')
                {
                    RecordReadsCounter++;
                    return Encoding.ASCII.GetChars(recordBuffer.Take(recordBufferLengh).ToArray());
                }
            }
            throw new Exception("Data in file doesn't match record type. Sequence too long");
        }

        public void WriteRecord(char[] savedRecord)
        {
            CheckActive();
            SetWriteMode();

            if (savedRecord.Length > MaxRecordLength)
            {
                throw new Exception("Record too long");
            }
            if (savedRecord[savedRecord.Length - 1] != '\0')
            {
                throw new Exception("String has to end with \\0 value");
            }
            
            
            byte[] byteRecord = Encoding.ASCII.GetBytes(savedRecord);
            
            if (_bufferSize - _bufferIndex > savedRecord.Length)
            {
                Array.Copy(byteRecord, 0, _buffer, _bufferIndex, byteRecord.Length);
                _bufferIndex += savedRecord.Length;
            }
            else if (_bufferSize - _bufferIndex == savedRecord.Length)
            {
                Array.Copy(byteRecord, 0, _buffer, _bufferIndex, byteRecord.Length);
                _stream.Write(_buffer);
                DiscWritesCounter++;
                _stream.Flush();

                _bufferIndex = 0;
            }
            else
            {
                byte[] byteRecordFirstPart = byteRecord.Take(_bufferSize - _bufferIndex).ToArray(); 
                byte[] byteRecordSecondPart = byteRecord.Skip(_bufferSize - _bufferIndex).ToArray();

                Array.Copy(byteRecordFirstPart, 0, _buffer, _bufferIndex, byteRecordFirstPart.Length);

                _stream.Write(_buffer);
                DiscWritesCounter++;
                _stream.Flush();

                _bufferIndex = 0;
                Array.Copy(byteRecordSecondPart, 0, _buffer, _bufferIndex, byteRecordSecondPart.Length);
                _bufferIndex += byteRecordSecondPart.Length;
            }
            RecordWritesCounter++;
        }
        public void ReturnToTapeBeginning()
        {
            CheckActive();
            _stream.Seek(0, SeekOrigin.Begin);
            _bufferIndex = 0;
            _buffer = new byte[_bufferSize];
        }
        public void ClearTape()
        {
            CheckActive();
            _stream.SetLength(0);
            _stream.Position = 0;
            _bufferIndex = 0;
            _buffer = new byte[_bufferSize];
        }

        public void SetReadMode()
        {
            CheckActive();
            if (!_readMode)
            {
                _stream.Write(_buffer.Take(_bufferIndex).ToArray());
                DiscReadsCounter++;
                _stream.Flush();

                _stream.Position = 0;
                _bufferIndex = 0;
                _bufferOccupancy = 0;
                _readMode = true;
            }
        }
        public void SetWriteMode() 
        {
            CheckActive();
            if (_readMode)
            {
                _readMode = false;
                ClearTape();
            }
        }
        public void PrintTapeData(char[]? alreadyLoadedShorterRecord = null)
        {
            CheckActive();
            SetReadMode();

            int savedDiscReadsCounter = DiscReadsCounter;
            int savedDiscWritesCounter = DiscWritesCounter;
            int savedRecordWritesCouter = RecordWritesCounter;
            int savedRecordReadsCounter = RecordReadsCounter;

            long streamIndex = _stream.Position;
            int bufferOccupancy = _bufferOccupancy;
            int bufferIndex = _bufferIndex;
            byte[] buffer = new byte[_bufferSize];
            _buffer.CopyTo(buffer, 0);

            char[]? record;
            char[]? previousRecord = null;
            int index = 1;


            if (alreadyLoadedShorterRecord != null)
            {
                record = alreadyLoadedShorterRecord;
            }
            else
            {
                record = ReadRecord();
            }

            Console.WriteLine($"\"{FileName}\": ");

            while (record != null)
            {
                if (previousRecord == null || TapeHandler.CompareCharSet(record, previousRecord) == -1)
                {
                    Console.Write($"{index++}. ");
                }
                Console.Write($" {new string(record)}"); //new string(record)
                previousRecord = record;
                record = ReadRecord();
                Console.WriteLine();

            }
            Console.WriteLine("\n");

            ReturnToTapeBeginning();

            DiscReadsCounter = savedDiscReadsCounter;
            DiscWritesCounter = savedDiscWritesCounter;
            RecordReadsCounter = savedRecordReadsCounter;
            RecordWritesCounter = savedRecordWritesCouter;
            _stream.Position = streamIndex;
            _bufferOccupancy = bufferOccupancy;
            _bufferIndex = bufferIndex;
            _buffer = buffer;
        }

        public void SwapFile(Tape OtherTape)
        {
            CheckActive();
            if (Object.ReferenceEquals(this, OtherTape))
            {
                return;
            }
            string otherFileName = OtherTape.FileName;
            string tempFileGUID = Guid.NewGuid().ToString();

            SetReadMode();
            this._stream.Close();

            OtherTape.SetReadMode();
            OtherTape._stream.Close();

            File.Move(FileName, tempFileGUID);
            File.Move(otherFileName, FileName);
            File.Move(tempFileGUID, otherFileName);

            this._stream = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite);
            OtherTape._stream = new FileStream(OtherTape.FileName, FileMode.Open, FileAccess.ReadWrite);
        }

        public void ResetDiscReadsCouter()
        {
            DiscReadsCounter = 0;
        }
        public void ResetDiscWritesCouter()
        {
            DiscWritesCounter = 0;
        }
        public void ResetRecordReadsCouter()
        {
            RecordReadsCounter = 0;
        }
        public void ResetRecordWritesCouter()
        {
            RecordWritesCounter = 0;
        }
    }
}
