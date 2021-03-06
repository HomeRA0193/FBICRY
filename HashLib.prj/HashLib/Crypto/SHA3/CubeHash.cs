using System;
using System.Linq;

namespace HashLib.Crypto.SHA3
{
	class CubeHash224 : CubeHash
	{
		public CubeHash224()
			: base(HashLib.HashSize.HashSize224)
		{
		}
	}

	class CubeHash256 : CubeHash
	{
		public CubeHash256()
			: base(HashLib.HashSize.HashSize256)
		{
		}
	}

	class CubeHash384 : CubeHash
	{
		public CubeHash384()
			: base(HashLib.HashSize.HashSize384)
		{
		}
	}

	class CubeHash512 : CubeHash
	{
		public CubeHash512()
			: base(HashLib.HashSize.HashSize512)
		{
		}
	}

	class CubeHash_1_8_512 : CubeHashCustom
	{
		public CubeHash_1_8_512()
			: base(HashLib.HashSize.HashSize512, 1, 8)
		{
		}
	}

	class CubeHash_1_1_512 : CubeHashCustom
	{
		public CubeHash_1_1_512()
			: base(HashLib.HashSize.HashSize512, 1, 1)
		{
		}
	}

	class CubeHash_8_1_512 : CubeHashCustom
	{
		public CubeHash_8_1_512()
			: base(HashLib.HashSize.HashSize512, 8, 1)
		{
		}
	}

	class CubeHash_8_64_512 : CubeHashCustom
	{
		public CubeHash_8_64_512()
			: base(HashLib.HashSize.HashSize512, 8, 64)
		{
		}
	}

	class CubeHashCustom : CubeHash
	{
		public CubeHashCustom(HashSize a_hashSize, int a_rounds = 16, int a_blockSize = 32)
			: base(a_hashSize, a_rounds, a_blockSize)
		{
		}
	}

	abstract class CubeHash : HashCryptoNotBuildIn
	{
		private static readonly uint[,,][] m_inits;
		private readonly int m_rounds;
		private readonly uint[] m_state = new uint[32];

		static CubeHash()
		{
			var hashes = new[] {28, 32, 48, 64};
			var rounds = new[] {1, 2, 4, 8, 16};
			var blocksizes = new[] {1, 2, 4, 8, 16, 32, 64, 128};
			var inits = new uint[rounds.Last() + 1,hashes.Last() + 1,blocksizes.Last() + 1][];
			var zeroes = new byte[blocksizes.Last()];

			foreach(int round in rounds)
			{
				foreach(int hashsize in hashes)
				{
					foreach(int blocksize in blocksizes)
					{
						CubeHash ch = new CubeHashCustom(HashLib.HashSize.HashSize256, round, blocksize);

						ch.m_state[0] = (uint)hashsize;
						ch.m_state[1] = (uint)blocksize;
						ch.m_state[2] = (uint)round;

						for(int i = 0; i < 10; i++)
							ch.TransformBlock(zeroes, 0);

						inits[round, hashsize, blocksize] = new uint[32];
						Array.Copy(ch.m_state, inits[round, hashsize, blocksize], 32);
					}
				}
			}

			m_inits = inits;
		}

		public CubeHash(HashSize a_hashSize, int a_rounds = 16, int a_blockSize = 32)
			: base((int)a_hashSize, a_blockSize)
		{
			if(a_blockSize > 128)
				throw new ArgumentOutOfRangeException();
			if(a_blockSize < 1)
				throw new ArgumentOutOfRangeException();
			if(a_rounds > 32)
				throw new NotImplementedException();
			if(a_rounds < 1)
				throw new ArgumentOutOfRangeException();

			m_rounds = a_rounds;

			Initialize();
		}

		protected override byte[] GetResult()
		{
			return Converters.ConvertUIntsToBytes(m_state, 0, HashSize / 4);
		}

		protected override void Finish()
		{
			var pad = new byte[BlockSize + 1];
			pad[0] = 0x80;

			TransformBytes(pad, 0, BlockSize - m_buffer.Pos);

			m_state[31] ^= 1;

			for(int i = 0; i < 10; i++)
				TransformBlock(pad, 1);
		}

		public override void Initialize()
		{
			if(m_inits != null)
			{
				uint[] state = m_inits[m_rounds, HashSize, BlockSize];

				if(state == null)
					throw new NotImplementedException();

				Array.Copy(state, m_state, 32);
			}

			base.Initialize();
		}

		protected override void TransformBlock(byte[] a_data, int a_index)
		{
			var temp = new uint[16];

			var state = new uint[32];
			Array.Copy(m_state, state, 32);

			uint[] data;
			if(BlockSize >= 4)
				data = Converters.ConvertBytesToUInts(a_data, a_index, BlockSize);
			else if(BlockSize == 2)
			{
				data = new uint[1];
				data[0] = BitConverter.ToUInt16(a_data, a_index);
			}
			else
			{
				data = new uint[1];
				data[0] = a_data[a_index];
			}


			for(int i = 0; i < data.Length; i++)
				state[i] ^= data[i];

			for(int r = 0; r < m_rounds; ++r)
			{
				state[16] += state[0];
				state[17] += state[1];
				state[18] += state[2];
				state[19] += state[3];
				state[20] += state[4];
				state[21] += state[5];
				state[22] += state[6];
				state[23] += state[7];
				state[24] += state[8];
				state[25] += state[9];
				state[26] += state[10];
				state[27] += state[11];
				state[28] += state[12];
				state[29] += state[13];
				state[30] += state[14];
				state[31] += state[15];

				temp[0 ^ 8] = state[0];
				temp[1 ^ 8] = state[1];
				temp[2 ^ 8] = state[2];
				temp[3 ^ 8] = state[3];
				temp[4 ^ 8] = state[4];
				temp[5 ^ 8] = state[5];
				temp[6 ^ 8] = state[6];
				temp[7 ^ 8] = state[7];
				temp[8 ^ 8] = state[8];
				temp[9 ^ 8] = state[9];
				temp[10 ^ 8] = state[10];
				temp[11 ^ 8] = state[11];
				temp[12 ^ 8] = state[12];
				temp[13 ^ 8] = state[13];
				temp[14 ^ 8] = state[14];
				temp[15 ^ 8] = state[15];

				for(int i = 0; i < 16; i++)
					state[i] = (temp[i] << 7) | (temp[i] >> 25);

				state[0] ^= state[16];
				state[1] ^= state[17];
				state[2] ^= state[18];
				state[3] ^= state[19];
				state[4] ^= state[20];
				state[5] ^= state[21];
				state[6] ^= state[22];
				state[7] ^= state[23];
				state[8] ^= state[24];
				state[9] ^= state[25];
				state[10] ^= state[26];
				state[11] ^= state[27];
				state[12] ^= state[28];
				state[13] ^= state[29];
				state[14] ^= state[30];
				state[15] ^= state[31];

				temp[0 ^ 2] = state[16];
				temp[1 ^ 2] = state[17];
				temp[2 ^ 2] = state[18];
				temp[3 ^ 2] = state[19];
				temp[4 ^ 2] = state[20];
				temp[5 ^ 2] = state[21];
				temp[6 ^ 2] = state[22];
				temp[7 ^ 2] = state[23];
				temp[8 ^ 2] = state[24];
				temp[9 ^ 2] = state[25];
				temp[10 ^ 2] = state[26];
				temp[11 ^ 2] = state[27];
				temp[12 ^ 2] = state[28];
				temp[13 ^ 2] = state[29];
				temp[14 ^ 2] = state[30];
				temp[15 ^ 2] = state[31];

				state[16] = temp[0];
				state[17] = temp[1];
				state[18] = temp[2];
				state[19] = temp[3];
				state[20] = temp[4];
				state[21] = temp[5];
				state[22] = temp[6];
				state[23] = temp[7];
				state[24] = temp[8];
				state[25] = temp[9];
				state[26] = temp[10];
				state[27] = temp[11];
				state[28] = temp[12];
				state[29] = temp[13];
				state[30] = temp[14];
				state[31] = temp[15];

				state[16] += state[0];
				state[17] += state[1];
				state[18] += state[2];
				state[19] += state[3];
				state[20] += state[4];
				state[21] += state[5];
				state[22] += state[6];
				state[23] += state[7];
				state[24] += state[8];
				state[25] += state[9];
				state[26] += state[10];
				state[27] += state[11];
				state[28] += state[12];
				state[29] += state[13];
				state[30] += state[14];
				state[31] += state[15];

				temp[0 ^ 4] = state[0];
				temp[1 ^ 4] = state[1];
				temp[2 ^ 4] = state[2];
				temp[3 ^ 4] = state[3];
				temp[4 ^ 4] = state[4];
				temp[5 ^ 4] = state[5];
				temp[6 ^ 4] = state[6];
				temp[7 ^ 4] = state[7];
				temp[8 ^ 4] = state[8];
				temp[9 ^ 4] = state[9];
				temp[10 ^ 4] = state[10];
				temp[11 ^ 4] = state[11];
				temp[12 ^ 4] = state[12];
				temp[13 ^ 4] = state[13];
				temp[14 ^ 4] = state[14];
				temp[15 ^ 4] = state[15];

				for(int i = 0; i < 16; i++)
					state[i] = (temp[i] << 11) | (temp[i] >> 21);

				state[0] ^= state[16];
				state[1] ^= state[17];
				state[2] ^= state[18];
				state[3] ^= state[19];
				state[4] ^= state[20];
				state[5] ^= state[21];
				state[6] ^= state[22];
				state[7] ^= state[23];
				state[8] ^= state[24];
				state[9] ^= state[25];
				state[10] ^= state[26];
				state[11] ^= state[27];
				state[12] ^= state[28];
				state[13] ^= state[29];
				state[14] ^= state[30];
				state[15] ^= state[31];

				temp[0 ^ 1] = state[16];
				temp[1 ^ 1] = state[17];
				temp[2 ^ 1] = state[18];
				temp[3 ^ 1] = state[19];
				temp[4 ^ 1] = state[20];
				temp[5 ^ 1] = state[21];
				temp[6 ^ 1] = state[22];
				temp[7 ^ 1] = state[23];
				temp[8 ^ 1] = state[24];
				temp[9 ^ 1] = state[25];
				temp[10 ^ 1] = state[26];
				temp[11 ^ 1] = state[27];
				temp[12 ^ 1] = state[28];
				temp[13 ^ 1] = state[29];
				temp[14 ^ 1] = state[30];
				temp[15 ^ 1] = state[31];

				state[16] = temp[0];
				state[17] = temp[1];
				state[18] = temp[2];
				state[19] = temp[3];
				state[20] = temp[4];
				state[21] = temp[5];
				state[22] = temp[6];
				state[23] = temp[7];
				state[24] = temp[8];
				state[25] = temp[9];
				state[26] = temp[10];
				state[27] = temp[11];
				state[28] = temp[12];
				state[29] = temp[13];
				state[30] = temp[14];
				state[31] = temp[15];
			}

			Array.Copy(state, m_state, 32);
		}
	}
}