
#pragma once 

#include "TestBase.h"
#include "Blake.h"

namespace CLRTest
{
    ref class BlakeTest : TestBase
    {
        private:

            Blake::Blake^ m_hash;

        protected:

            virtual void TransformBytes(array<byte>^ a_data, int a_index, int a_length) override
            {
                m_hash->TransformBytes(a_data, a_index, a_length);
            }

            virtual array<byte>^ ComputeBytes(array<byte>^ a_data) override
            {
                return m_hash->ComputeBytes(a_data);
            }

            virtual array<byte>^ TransformFinal() override
            {
                return m_hash->TransformFinal();
            }

            virtual void CreateHash(int a_hashLenBits) override
            {
                if (a_hashLenBits <= 256)
                    m_hash = gcnew Blake::Blake32(a_hashLenBits/8);
                else
                    m_hash = gcnew Blake::Blake64(a_hashLenBits/8);
            }

            virtual void InitializeHash() override
            {
                m_hash->Initialize();
            }

            virtual String^ GetTestVectorsDir() override
            {
                return "Blake";
            }

            virtual String^ GetTestName() override
            {
                return "Blake-CLR";
            }

            virtual int GetMaxBufferSize() override
            {
                return 128;
            }

        public: 

            static void DoTest()
            {
                BlakeTest^ test = gcnew BlakeTest();
                test->Test();
            }
    };

    ref class BlakeCSharpTest : TestBase
    {
        private:

            IHash^ m_hash;

        protected:

            virtual void TransformBytes(array<byte>^ a_data, int a_index, int a_length) override
            {
                m_hash->TransformBytes(a_data, a_index, a_length);
            }

            virtual array<byte>^ ComputeBytes(array<byte>^ a_data) override
            {
                return m_hash->ComputeBytes(a_data)->GetBytes();
            }

            virtual array<byte>^ TransformFinal() override
            {
                return m_hash->TransformFinal()->GetBytes();
            }

            virtual void CreateHash(int a_hashLenBits) override
            {
                m_hash = HashLib::HashFactory::Crypto::SHA3::CreateBlake(HashLib::HashSize(a_hashLenBits/8));
            }

            virtual void InitializeHash() override
            {
                m_hash->Initialize();
            }

            virtual String^ GetTestVectorsDir() override
            {
                return "Blake";
            }

            virtual String^ GetTestName() override
            {
                return "Blake-CSharp";
            }

            virtual int GetMaxBufferSize() override
            {
                return 128;
            }

        public: 

            static void DoTest()
            {
                BlakeCSharpTest^ test = gcnew BlakeCSharpTest();
                test->Test();
            }
    };
}
