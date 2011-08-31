
#pragma once 

#include "TestBase.h"
#include "JH.h"

namespace CLRTest
{
    ref class JHTest : TestBase
    {
        private:

            JH::JH^ m_hash;

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
                m_hash = gcnew JH::JH((JH::JHHashSize)(a_hashLenBits/8));
            }

            virtual void InitializeHash() override
            {
                m_hash->Initialize();
            }

            virtual String^ GetTestVectorsDir() override
            {
                return "JH";
            }

            virtual String^ GetTestName() override
            {
                return "JH-CLR";
            }

            virtual int GetMaxBufferSize() override
            {
                return 64;
            }

        public: 

            static void DoTest()
            {
                JHTest^ test = gcnew JHTest();
                test->Test();
            }
    };

    ref class JHCSharpTest : TestBase
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
                m_hash = HashLib::HashFactory::Crypto::SHA3::CreateJH((HashLib::HashSize)(a_hashLenBits/8));
            }

            virtual void InitializeHash() override
            {
                m_hash->Initialize();
            }

            virtual String^ GetTestVectorsDir() override
            {
                return "JH";
            }

            virtual String^ GetTestName() override
            {
                return "JH-CSharp";
            }

            virtual int GetMaxBufferSize() override
            {
                return 64;
            }

        public: 

            static void DoTest()
            {
                JHCSharpTest^ test = gcnew JHCSharpTest();
                test->Test();
            }
    };
}
