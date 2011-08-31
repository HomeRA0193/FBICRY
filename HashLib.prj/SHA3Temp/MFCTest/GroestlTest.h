
#pragma once

#include "TestBase.h"
#include "Groestl-org.h"
#include "Groestl.h"

namespace MFCTest
{
    class GroestlTest : public TestBase
    {
        private:

            Groestl::GroestlBase* m_hash;

        protected:
    
            virtual CString GetTestVectorsDir()
            {
                return "Groestl";
            }

            virtual CString GetTestName()
            {
                return "Groestl";
            }

            virtual int GetMaxBufferSize()
            {
                return 128;
            }

            virtual void TransformBytes(byte* a_data, int a_index, int a_length)
            {
                m_hash->TransformBytes(a_data, a_index, a_length); 
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
                if (a_hashLenBits <= 256)
                    m_hash = new Groestl::Groestl256Base(a_hashLenBits/8);
                else
                    m_hash = new Groestl::Groestl512Base(a_hashLenBits/8);
            }

            virtual void InitializeHash()
            {
                m_hash->Initialize();
            }

        public:

            GroestlTest()
            {
                m_hash = nullptr;
            }

            ~GroestlTest()
            {
                delete m_hash;
            }
    };

    class GroestlOrgTest : public TestBase
    {
        private:

            GroestlOrg::hashState m_hashState;

        protected:
    
            virtual CString GetTestVectorsDir()
            {
                return "Groestl";
            }

            virtual CString GetTestName()
            {
                return "Groestl-Org";
            }

            virtual int GetMaxBufferSize()
            {
                return 128;
            }

            virtual void TransformBytes(byte* a_data, int a_index, int a_length)
            {
                GroestlOrg::Update(&m_hashState, a_data + a_index, a_length*8); 
            }

            virtual byte* ComputeBytes(byte* a_data, int a_length)
            {
                byte* out = new byte[m_hashState.hashbitlen/8];
                GroestlOrg::Hash(m_hashState.hashbitlen, a_data, a_length*8, out);
                return out;
            }

            virtual byte* TransformFinal()
            {
                byte* out = new byte[m_hashState.hashbitlen/8];
                GroestlOrg::Final(&m_hashState, out);
                return out;
            }

            virtual void CreateHash(int a_hashLenBits)
            {
                GroestlOrg::Init(&m_hashState, a_hashLenBits);
            }

            virtual void InitializeHash()
            {
                GroestlOrg::Init(&m_hashState, m_hashState.hashbitlen);
            }
    };
}
