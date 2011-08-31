
#pragma once

#include "CubeHash.h"
#include "CubeHash-org.h"
#include "TestBase.h"

namespace MFCTest
{
    class CubeHashTest : public TestBase
    {
        private:

            CubeHash::CubeHash* m_hash;
            int m_blockSize;
            int m_rounds;

        protected:

            virtual CString GetTestVectorsDir()
            {
                CString str;
                str.Format("CubeHash-%d-%d", m_rounds, m_blockSize);
                return str;
            }

            virtual CString GetTestName()
            {
                CString str;
                str.Format("CubeHash-%d-%d", m_rounds, m_blockSize);
                return str;
            }

            virtual int GetMaxBufferSize()
            {
                return 32;
            }

            virtual void TransformBytes(byte* a_data, int a_index, int a_length)
            {
                m_hash->TransformBytes(a_data + a_index, a_length); 
            }

            virtual byte* ComputeBytes(byte* a_data, int a_length)
            {
                return m_hash->ComputeBytes(a_data, a_length);
            }

            virtual byte* TransformFinal()
            {
                return m_hash->TransformFinal();
            }

            virtual void CreateHash(int a_hashLenBits)
            {
                delete m_hash;
                m_hash = new CubeHash::CubeHash(a_hashLenBits/8, m_rounds, m_blockSize);
            }

            virtual void InitializeHash()
            {
                m_hash->Initialize();
            }

        public:

            CubeHashTest(int a_rounds, int a_blockSize)
            {
                m_hash = nullptr;
                m_blockSize = a_blockSize;
                m_rounds = a_rounds;
            }
    };

    class CubeHashOrgTest : public TestBase
    {
        private:

            CubeHashOrg::hashState m_hashState;

        protected:
    
            virtual CString GetTestVectorsDir()
            {
                return "CubeHash-16-32";
            }

            virtual CString GetTestName()
            {
                return "CubeHash-16-32-Org";
            }

            virtual int GetMaxBufferSize()
            {
                return 32;
            }

            virtual void TransformBytes(byte* a_data, int a_index, int a_length)
            {
                CubeHashOrg::Update(&m_hashState, a_data + a_index, a_length*8); 
            }

            virtual byte* ComputeBytes(byte* a_data, int a_length)
            {
                byte* out = new byte[m_hashState.hashbitlen/8];
                CubeHashOrg::Hash(m_hashState.hashbitlen, a_data, a_length*8, out);
                return out;
            }

            virtual byte* TransformFinal()
            {
                byte* out = new byte[m_hashState.hashbitlen/8];
                CubeHashOrg::Final(&m_hashState, out);
                return out;
            }

            virtual void CreateHash(int a_hashLenBits)
            {
                CubeHashOrg::Init(&m_hashState, a_hashLenBits);
            }

            virtual void InitializeHash()
            {
                CubeHashOrg::Init(&m_hashState, m_hashState.hashbitlen);
            }
    };
}
