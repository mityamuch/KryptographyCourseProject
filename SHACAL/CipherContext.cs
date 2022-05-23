using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHACAL
{
    public class CipherContext
    {
        #region Nested

        public enum Mode
        {
            ECB,
            CBC,
            CFB,
            OFB,
            CTR,
            RD,
            RDH
        };

        #endregion
        private readonly ICrypto _crypto;
        private readonly Mode _mode;
        private byte[] _InitializationVector;
        static int _BlockSize;//bytes ;20 for shacal
        private String _param;
        public CipherContext(ICrypto crypto,int Blocksize,Mode mode, byte[] vector)
        {
            _crypto = crypto ?? throw new ArgumentNullException(nameof(crypto));
            _mode = mode;
            _InitializationVector = vector;
            _BlockSize = Blocksize;
        }

        public CipherContext(ICrypto crypto, int Blocksize, Mode mode, byte[] vector, String str)
        {
            _crypto = crypto ?? throw new ArgumentNullException(nameof(crypto));
            _mode = mode;
            _InitializationVector = vector;
            _param = str;
        }

        private List<byte[]> GetListFromArrayWithBlockSizeLength(Byte[] result)
        {
            var resultList = new List<Byte[]>();

            for (var i = 0; i < result.Length / _BlockSize; i++)
            {
                resultList.Add(new byte[_BlockSize]);
                for (var j = 0; j < _BlockSize; j++)
                {
                    resultList[i][j] = result[i * _BlockSize + j];
                }
            }

            return resultList;
        }


        public byte[] Encrypt(byte[] data)
        {
            byte[] res = MakePaddingPKCS7(data);
            List<byte[]> blocks = new List<byte[]>();

            for (int i = 0; i < res.Length / _BlockSize; i++)
            {
                blocks.Add(null);
            }

            switch (_mode)
            {
                case Mode.ECB:
                    {
                        byte[] block = new byte[_BlockSize];
                        List<byte[]> blockList = GetListFromArrayWithBlockSizeLength(res);

                        Parallel.For(0, res.Length / _BlockSize, i =>

                            blocks[i] = _crypto.Encrypt(blockList[i])
                        );
                        break;
                    }

                case Mode.CBC:
                    {
                        byte[] prevBlock = new byte[_BlockSize];
                        byte[] curBlock = new byte[_BlockSize];
                        Array.Copy(_InitializationVector, prevBlock, prevBlock.Length);

                        for (int i = 0; i < res.Length / _BlockSize; i++)
                        {
                            Array.Copy(res, i * _BlockSize, curBlock, 0, _BlockSize);
                            blocks[i] = _crypto.Encrypt(AuxilarityFunctions.XOR(curBlock, prevBlock));
                            Array.Copy(blocks[i], prevBlock, _BlockSize);
                        }
                        break;
                    }

                case Mode.CFB:
                    {
                        byte[] prevBlock = new byte[_BlockSize];
                        byte[] curBlock = new byte[_BlockSize];
                        Array.Copy(_InitializationVector, prevBlock, prevBlock.Length);
                        for (int i = 0; i < res.Length / _BlockSize; i++)
                        {
                            Array.Copy(res, i * _BlockSize, curBlock, 0, _BlockSize);
                            blocks[i] = AuxilarityFunctions.XOR(_crypto.Encrypt(prevBlock), curBlock);
                            Array.Copy(blocks[i], prevBlock, _BlockSize);
                        }
                        break;
                    }

                case Mode.OFB:
                    {
                        byte[] prevBlock = new byte[_BlockSize];
                        byte[] curBlock = new byte[_BlockSize];
                        byte[] encryptBlock = new byte[_BlockSize];
                        Array.Copy(_InitializationVector, prevBlock, prevBlock.Length);
                        for (int i = 0; i < res.Length / _BlockSize; i++)
                        {
                            Array.Copy(res, i * _BlockSize, curBlock, 0, _BlockSize);
                            encryptBlock = _crypto.Encrypt(prevBlock);
                            blocks[i] = AuxilarityFunctions.XOR(encryptBlock, curBlock);
                            Array.Copy(encryptBlock, prevBlock, _BlockSize);
                        }
                        break;
                    }

                case Mode.CTR:
                    {
                        var copyIV = new byte[8];
                        _InitializationVector.CopyTo(copyIV, 0);
                        var counter = BitConverter.ToUInt64(copyIV, 0);
                        List<byte[]> blockList = GetListFromArrayWithBlockSizeLength(res);
                        List<byte[]> counterList = new List<byte[]>();
                        for (int i = 0; i < res.Length / _BlockSize; i++)
                        {
                            counterList.Add(copyIV);
                            counter++;
                            copyIV = BitConverter.GetBytes(counter);
                        }

                        Parallel.For(0, res.Length / _BlockSize, i =>

                            blocks[i] = AuxilarityFunctions.XOR(_crypto.Encrypt(counterList[i]), blockList[i])
                        );
                        break;
                    }

                case Mode.RD:
                    {
                        byte[] DeltaArr = new byte[8];
                        Array.Copy(_InitializationVector, 8, DeltaArr, 0, _BlockSize);
                        var Delta = BitConverter.ToUInt64(DeltaArr);
                        var copyIV = new byte[8];
                        Array.Copy(_InitializationVector, 0, copyIV, 0, _BlockSize);
                        var IV = BitConverter.ToUInt64(copyIV);
                        blocks.Add(null);
                        blocks[0] = _crypto.Encrypt(copyIV);

                        List<byte[]> blockList = GetListFromArrayWithBlockSizeLength(res);
                        List<byte[]> counterList = new List<byte[]>();
                        for (int i = 0; i < res.Length / _BlockSize; i++)
                        {
                            counterList.Add(copyIV);
                            IV += Delta;
                            copyIV = BitConverter.GetBytes(IV);
                        }

                        Parallel.For(0, res.Length / _BlockSize, i =>

                            blocks[i + 1] = _crypto.Encrypt(AuxilarityFunctions.XOR(counterList[i], blockList[i]))
                        );
                        break;
                    }

                case Mode.RDH:
                    {
                        byte[] DeltaArr = new byte[8];
                        Array.Copy(_InitializationVector, 8, DeltaArr, 0, _BlockSize);
                        var copyIV = new byte[8];
                        Array.Copy(_InitializationVector, 0, copyIV, 0, _BlockSize);
                        var IV = BitConverter.ToUInt64(copyIV);
                        var Delta = BitConverter.ToUInt64(DeltaArr);
                        blocks.Add(null);
                        blocks[0] = _crypto.Encrypt(copyIV);
                        blocks.Add(null);
                        blocks[1] = AuxilarityFunctions.XOR(copyIV, MakePaddingPKCS7(BitConverter.GetBytes(_param.GetHashCode())));


                        List<byte[]> blockList = GetListFromArrayWithBlockSizeLength(res);
                        List<byte[]> counterList = new List<byte[]>();
                        for (int i = 0; i < res.Length / _BlockSize; i++)
                        {
                            IV += Delta;
                            copyIV = BitConverter.GetBytes(IV);
                            counterList.Add(copyIV);
                        }

                        Parallel.For(0, res.Length / _BlockSize, i =>

                            blocks[i + 2] = _crypto.Encrypt(AuxilarityFunctions.XOR(counterList[i], blockList[i]))
                        );
                        break;
                    }
            }
            return MakeArrayFromList(blocks);
        }

        public byte[] Decrypt(byte[] data)
        {
            List<byte[]> blocks = new List<byte[]>();
            for (int i = 0; i < data.Length / _BlockSize; i++)
            {
                blocks.Add(null);
            }
            switch (_mode)
            {
                case Mode.ECB:
                    {

                        List<byte[]> blockList = GetListFromArrayWithBlockSizeLength(data);

                        Parallel.For(0, data.Length / _BlockSize, i =>

                            blocks[i] = _crypto.Decrypt(blockList[i])
                        );
                        break;
                    }

                case Mode.CBC:
                    {
                        List<byte[]> blockList = GetListFromArrayWithBlockSizeLength(data);
                        blockList.Insert(0, _InitializationVector);

                        Parallel.For(0, data.Length / _BlockSize, i =>

                            blocks[i] = AuxilarityFunctions.XOR(blockList[i], _crypto.Decrypt(blockList[i + 1]))
                        );
                        break;
                    }

                case Mode.CFB:
                    {
                        List<byte[]> blockList = GetListFromArrayWithBlockSizeLength(data);
                        blockList.Insert(0, _InitializationVector);

                        Parallel.For(0, data.Length / _BlockSize, i =>

                            blocks[i] = AuxilarityFunctions.XOR(_crypto.Decrypt(blockList[i]), blockList[i + 1])
                        );
                        break;
                    }

                case Mode.OFB:
                    {
                        byte[] prevBlock = new byte[_BlockSize];
                        byte[] curBlock = new byte[_BlockSize];
                        byte[] decryptedBlock = new byte[_BlockSize];
                        Array.Copy(_InitializationVector, prevBlock, prevBlock.Length);
                        for (int i = 0; i < data.Length / _BlockSize; i++)
                        {
                            Array.Copy(data, i * _BlockSize, curBlock, 0, _BlockSize);
                            decryptedBlock = _crypto.Encrypt(prevBlock);
                            blocks.Add(AuxilarityFunctions.XOR(decryptedBlock, curBlock));
                            Array.Copy(decryptedBlock, prevBlock, _BlockSize);
                        }
                        break;
                    }

                case Mode.CTR:
                    {
                        var counter = BitConverter.ToUInt64(_InitializationVector, 0);
                        byte[] curBlock = new byte[_BlockSize];
                        List<byte[]> blockList = GetListFromArrayWithBlockSizeLength(data);
                        List<byte[]> counterList = new List<byte[]>();

                        for (int i = 0; i < data.Length / _BlockSize; i++)
                        {
                            counterList.Add(_InitializationVector);
                            counter++;
                            _InitializationVector = BitConverter.GetBytes(counter);
                        }

                        Parallel.For(0, data.Length / _BlockSize, i =>

                            blocks[i] = AuxilarityFunctions.XOR(_crypto.Encrypt(counterList[i]), blockList[i])
                        );
                        break;
                    }

                case Mode.RD:
                    {
                        byte[] curBlock = new byte[_BlockSize];
                        byte[] DeltaArr = new byte[8];
                        Array.Copy(_InitializationVector, _InitializationVector.Length / 2, DeltaArr, 0, _BlockSize);
                        var copyIV = new byte[8];
                        var Delta = BitConverter.ToUInt64(DeltaArr);
                        Array.Copy(data, 0, curBlock, 0, _BlockSize);
                        copyIV = _crypto.Decrypt(curBlock);
                        var IV = BitConverter.ToUInt64(copyIV);

                        List<byte[]> blockList = GetListFromArrayWithBlockSizeLength(data);
                        List<byte[]> counterList = new List<byte[]>();
                        for (int i = 0; i < data.Length / _BlockSize; i++)
                        {
                            counterList.Add(copyIV);
                            IV += Delta;
                            copyIV = BitConverter.GetBytes(IV);
                        }

                        Parallel.For(1, data.Length / _BlockSize, i =>

                            blocks[i - 1] = AuxilarityFunctions.XOR(_crypto.Decrypt(blockList[i]), counterList[i - 1])
                        );
                        blocks.RemoveAt(blocks.Count - 1);
                        break;
                    }

                case Mode.RDH:
                    {
                        byte[] curBlock = new byte[_BlockSize];
                        byte[] DeltaArr = new byte[8];
                        Array.Copy(_InitializationVector, _InitializationVector.Length / 2, DeltaArr, 0, _BlockSize);
                        var copyIV = new byte[8];
                        var Delta = BitConverter.ToUInt64(DeltaArr);
                        Array.Copy(data, 0, curBlock, 0, _BlockSize);
                        copyIV = _crypto.Decrypt(curBlock);
                        var IV = BitConverter.ToUInt64(copyIV);
                        List<byte[]> blockList = GetListFromArrayWithBlockSizeLength(data);
                        if (!AuxilarityFunctions.XOR(copyIV, MakePaddingPKCS7(BitConverter.GetBytes(_param.GetHashCode()))).SequenceEqual(blockList[1]))
                            break;

                        List<byte[]> counterList = new List<byte[]>();
                        for (int i = 0; i < data.Length / _BlockSize - 2; i++)
                        {
                            IV += Delta;
                            copyIV = BitConverter.GetBytes(IV);
                            counterList.Add(copyIV);
                        }

                        Parallel.For(2, data.Length / _BlockSize, i =>

                            blocks[i - 2] = AuxilarityFunctions.XOR(_crypto.Decrypt(blockList[i]), counterList[i - 2])
                        );
                        blocks.RemoveAt(blocks.Count - 1);
                        blocks.RemoveAt(blocks.Count - 1);
                        break;
                    }
            }
            // padding 
            byte[] array = MakeArrayFromList(blocks);
            byte extraBlocks = array[array.Length - 1];
            var res = new byte[array.Length - extraBlocks];
            Array.Copy(array, res, res.Length);
            return res;
        }

        private byte[] MakePaddingPKCS7(byte[] data)
        {
            byte mod = (byte)(_BlockSize - data.Length % _BlockSize);
            mod = (byte)(mod == 0 ? _BlockSize : mod);
            byte[] addedData = new byte[data.Length + mod];
            Array.Copy(data, addedData, data.Length);
            Array.Fill(addedData, mod, data.Length, mod);
            return addedData;
        }


        private byte[] MakeArrayFromList(List<byte[]> data)
        {
            byte[] res = new byte[_BlockSize * data.Count];
            for (int i = 0; i < data.Count; i++)
            {
                Array.Copy(data[i], 0, res, i * _BlockSize, _BlockSize);
            }
            return res;
        }







    }
}
