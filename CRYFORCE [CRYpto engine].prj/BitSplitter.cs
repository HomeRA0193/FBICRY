﻿#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#endregion

namespace CRYFORCE.Engine
{
    /// <summary>
    ///   Класс разбиения/склеивания файлов на битовые потоки
    /// </summary>
    public class BitSplitter : IDisposable
    {
        #region Static

        /// <summary>
        /// Метод разбиения группы из 8 байт на набор "битовых" байт
        /// </summary>
        /// <param name="bytesIn"> Исходный набор байт. </param>
        /// <param name="bytesOut"> Массив "битовых" байт. </param>
        /// <returns> Массив "битовых" байт. </returns>
        public static void Split8Bytes(byte[] bytesIn, byte[] bytesOut)
        {
            // Каждый i-ый байт выходного потока формируется из i-ых бит
            for(int i = 0; i < NBITS; i++)
            {
                int b = 0x00;
                for(int j = 0; j < NBITS; j++)
                {
                    b |= ((bytesIn[j] >> i) & 0x01) << j;
                }
                bytesOut[i] = (byte)b;
            }
        }

        #endregion Static

        #region Constants

        /// <summary>
        /// Количество битов в байте.
        /// </summary>
        private const int NBITS = 8;

        /// <summary>
        /// Размер буфера в ОЗУ под каждый поток.
        /// </summary>
        private const int DEFAULT_BUFFER_SIZE_PER_STREAM = 16 * 1024 * 1024; // 16 мегабайт

        #endregion Constants

        #region Data

        /// <summary>
        /// Сущность для работы с перестановками битовой карты (для перемешивания битов).
        /// </summary>
        private readonly BitMaps _bitMaps;

        /// <summary>
        /// Битовые потоки.
        /// </summary>
        private Stream[] _bitStreams;

        #endregion Data

        #region Events

        #endregion Events

        #region .ctor

        /// <summary>
        /// Конструктор с параметрами
        /// </summary>
        /// <param name="key1"> Ключ для первого прохода шифрования. </param>
        /// <param name="key2"> Ключ для второго прохода шифрования. </param>
        /// <param name="workInMemory"> Работать в ОЗУ? </param>
        public BitSplitter(byte[] key1, byte[] key2, bool workInMemory)
        {
            BufferSizePerStream = DEFAULT_BUFFER_SIZE_PER_STREAM;
            RndSeed = DateTime.Now.Ticks.GetHashCode();
            _bitMaps = new BitMaps(key1, key2);

            // Работаем так, как желает пользователь
            Initialize(CryforceUtilities.GetRandomFilenames(NBITS, NBITS, RndSeed).Select(item => item + ".jpg").ToArray(), workInMemory);
        }

        /// <summary>
        /// Конструктор с параметрами
        /// </summary>
        /// <param name="bitStreamsNames"> Имена битовых потоков. </param>
        /// <param name="key1"> Ключ для первого прохода шифрования. </param>
        /// <param name="key2"> Ключ для второго прохода шифрования. </param>
        /// <param name="workInMemory"> Работать в ОЗУ? </param>
        public BitSplitter(IEnumerable<string> bitStreamsNames, byte[] key1, byte[] key2, bool workInMemory)
        {
            BufferSizePerStream = DEFAULT_BUFFER_SIZE_PER_STREAM;
            RndSeed = DateTime.Now.Ticks.GetHashCode();
            _bitMaps = new BitMaps(key1, key2);

            // Работаем так, как желает пользователь
            Initialize(bitStreamsNames, workInMemory);
        }

        /// <summary>
        /// IDisposable
        /// </summary>
        public void Dispose()
        {
            // Очищаем секретные данные
            ClearAndClose();

            // Финализатор для данного объекта не запускать!
            GC.SuppressFinalize(this);
        }

        #endregion .ctor

        #region Properties

        /// <summary>
        /// Ссылка на основной класс ядра шифрования.
        /// </summary>
        public IMessage msg { get; set; }

        /// <summary>
        /// Работаем в ОЗУ?
        /// </summary>
        public bool WorkInMemory { get; set; }

        /// <summary>
        /// Размер буфера в ОЗУ под каждый поток.
        /// </summary>
        public int BufferSizePerStream { get; set; }

        /// <summary>
        /// Инициализирующее значение генератора случайных чисел.
        /// </summary>
        public int RndSeed { get; set; }

        /// <summary>
        /// Затирать выходной поток нулями?
        /// </summary>
        public bool ZeroOut { get; set; }

        /// <summary>
        /// Экземпляр класса инициализирован?
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Имена битовых потоков.
        /// </summary>
        public string[] BitStreamsNames { get; private set; }

        #endregion Properties

        #region Private

        #endregion Private

        #region Protected

        #endregion Protected

        #region Public

        /// <summary>
        /// Инициализация экземпляра класса
        /// </summary>
        /// <param name="bitStreamsNames"> Имена битовых потоков. </param>
        /// <param name="workInMemory"> Работать в ОЗУ? </param>
        public void Initialize(IEnumerable<string> bitStreamsNames, bool workInMemory)
        {
            if(bitStreamsNames.Count() < NBITS)
            {
                throw new Exception("BitSplitter::Initialize() ==> bitStreamsNames count is too small!");
            }

            BitStreamsNames = bitStreamsNames.ToArray();
            WorkInMemory = workInMemory;

            // Выделяем память под массив битовых потоков
            _bitStreams = WorkInMemory ? new MemoryStream[NBITS] : (Stream[])new BufferedStream[NBITS];

            // ...для всех потоков...
            for(int i = 0; i < _bitStreams.Length; i++)
            {
                //...готовим их к работе...
                _bitStreams[i] = CryforceUtilities.PrepareOutputStream(msg, BitStreamsNames[i], BufferSizePerStream, ZeroOut, WorkInMemory, RndSeed);
            }

            // Указываем, что инициализация прошла успешно
            IsInitialized = true;
        }

        /// <summary>
        /// Разбиение на битовые потоки с последующим "склеиванием" в единый поток бит
        /// </summary>
        /// <param name="inputStream"> Входной поток. </param>
        /// <param name="outputStream"> Выходной поток. </param>
        public void SplitToBitstream(Stream inputStream, Stream outputStream)
        {
            if(!IsInitialized)
            {
                throw new Exception("BitSplitter::SplitToBitstream() ==> BitSplitter is not initialized!");
            }

            // Исходный,...
            inputStream.Seek(0, SeekOrigin.Begin);

            //...битовые...
            for(int i = 0; i < NBITS; i++)
            {
                _bitStreams[i].Seek(0, SeekOrigin.Begin);
            }

            //...и целевой поток устанавливаем на начало
            outputStream.Seek(0, SeekOrigin.Begin);

            // Устанавливаем начальные параметры для успешного проведения итераций
            long remaining = inputStream.Length;

            // Создаем буфер для работы блоками по 8 байт
            var bytesIn = new byte[NBITS];
            var bytesOut = new byte[NBITS];

            int read; // Счетчик количества считанных байт
            int toRead; // Счетчик количества байт, которые нужно считать

            // Вычисляем выравнивание до 8 байт
            var align8 = (byte)(inputStream.Length % NBITS);

            // Если выравнивание не требуется - писать в выходной поток ничего не будем
            if(align8 != 0)
            {
                // Вычисляем выравнивание входного потока до 8 байт (маскируя его дополнительными битами)...
                var rnd = new Random(RndSeed ^ DateTime.Now.Ticks.GetHashCode());
                var align8Rnd = (byte)(align8 | (0xF8 & (rnd.Next(0, 31) << 3)));

                //...и пишем его в выходной поток
                outputStream.WriteByte(align8Rnd);

                // Первый блок данных требуется считывать особым образом - так, чтобы учесть невыровненность по границе 8 байт -
                // для этого заполняем его случайными данными и пишем в него реальные данные из исходного потока, начиная с некоторой позиции
                rnd.NextBytes(bytesIn);

                read = 0;
                toRead = align8;
                while((toRead -= (read += inputStream.Read(bytesIn, read, toRead))) != 0) ;
                Split8Bytes(bytesIn, bytesOut); // Bit-splitting...

                // Запись соответствующих бит на свои места в битовые потоки
                int[] bitmap = _bitMaps.GetNextBitmap();
                for(int i = 0; i < NBITS; i++)
                {
                    _bitStreams[i].WriteByte(bytesOut[bitmap[i]]);
                }

                // Учитываем обработанный объем
                remaining -= align8;
            }

            // Рассчитываем количество итераций на один процент
            long nItersTotal = remaining / NBITS;
            long itersForPercent = nItersTotal / 100;
            itersForPercent = (itersForPercent != 0) ? itersForPercent : 1; // Минимум - 1 итерация на процент!

            long nIter = 0;

            // Пока есть объем для обработки...
            while(remaining > 0)
            {
                read = 0;
                toRead = NBITS;
                while((toRead -= (read += inputStream.Read(bytesIn, read, toRead))) != 0) ;
                Split8Bytes(bytesIn, bytesOut); // Bit-splitting...

                // Запись соответствующих бит на свои места в битовые потоки
                int[] bitmap = _bitMaps.GetNextBitmap();
                for(int i = 0; i < NBITS; i++)
                {
                    _bitStreams[i].WriteByte(bytesOut[bitmap[i]]);
                }

                // Учитываем обработанный объем
                remaining -= NBITS;

                // Выводим прогресс
                if(++nIter % itersForPercent == 0)
                {
                    if(msg != null) msg.Progress("SplitToBitstream (1/2)", (nIter / (double)nItersTotal) * 100, true);
                }
            }

            // Содержимое всех битовых потоков переносим в выходной поток
            for(int i = 0; i < NBITS; i++)
            {
                _bitStreams[i].Seek(0, SeekOrigin.Begin);
                _bitStreams[i].CopyTo(outputStream);
                if(msg != null) msg.Progress("SplitToBitstream (2/2)", (i / (double)NBITS) * 100, true);
            }

            // Чистим данные...
            CryforceUtilities.ClearArray(bytesIn);
            CryforceUtilities.ClearArray(bytesOut);

            // Устанавливаем потоки на начальные позиции...
            inputStream.Seek(0, SeekOrigin.Begin);
            outputStream.Seek(0, SeekOrigin.Begin);

            // Синхронизируем буфер с физическим носителем...
            outputStream.Flush();

            // Вывод прогресса...
            if(msg != null) msg.Progress("BitSplitter", 100);
        }

        /// <summary>
        /// Считывание из единого битового потока с последующим восстановлением порядка следования бит
        /// </summary>
        /// <param name="inputStream"> Входной поток. </param>
        /// <param name="outputStream"> Выходной поток. </param>
        public void UnsplitFromBitstream(Stream inputStream, Stream outputStream)
        {
            if(!IsInitialized)
            {
                throw new Exception("BitSplitter::UnsplitFromBitstream() ==> BitSplitter is not initialized!");
            }

            // Исходный,...
            inputStream.Seek(0, SeekOrigin.Begin);

            //...битовые...
            for(int i = 0; i < NBITS; i++)
            {
                _bitStreams[i].Seek(0, SeekOrigin.Begin);
            }

            //...и целевые потоки устанавливаем на начало
            outputStream.Seek(0, SeekOrigin.Begin);

            // Выделяем временный буфер
            var buffer = new byte[BufferSizePerStream];

            // Создаем буфер для работы блоками по 8 байт
            var bytesIn = new byte[NBITS];
            var bytesOut = new byte[NBITS];

            // Вычисляем выравнивание входного потока до 8 байт (отбрасывая маскирующие биты)...
            byte align8;

            // Количество байт для копирования в соотв. битстрим
            long remaining;

            // Автодетект выравнивания
            if(inputStream.Length % NBITS != 0)
            {
                align8 = (byte)(inputStream.ReadByte() & 0x07);
                remaining = inputStream.Length - 1;
            }
            else
            {
                align8 = 0;
                remaining = inputStream.Length;
            }

            // Вычисляем размер битстрима
            long bitstreamSize = remaining / NBITS;

            // Разбиваем исходный поток на 8 битстримов...
            for(int i = 0; i < NBITS; i++)
            {
                // Количество байт для копирования в соотв. битстрим
                long remaining2 = bitstreamSize;

                // Пока есть что копировать...
                while(remaining2 > 0)
                {
                    //...вычисляем количество байт для считывания
                    int toRead = (remaining2 < buffer.Length) ? (int)remaining2 : buffer.Length;
                    int read = 0;

                    // Наполняем временный буфер
                    while((toRead -= (read += inputStream.Read(buffer, read, toRead))) != 0) ;

                    // Учитываем отработанный объем
                    remaining2 -= read;

                    // Считали блок байт принадлежащий некоторому битстриму - помещаем его на свое место (bitstreamSize)...
                    _bitStreams[i].Write(buffer, 0, buffer.Length);
                }

                // Сообщаем о прогрессе
                if(msg != null) msg.Progress("UnsplitFromBitstream (1/2)", (i / (double)NBITS) * 100, true);
            }

            // Чистим временный буфер
            CryforceUtilities.ClearArray(buffer);

            // После того, как в битовых потоках восстановлены все байты - можно производить их разбор...

            // Устанавливаем исходные потоки на начало...
            for(int i = 0; i < NBITS; i++)
            {
                _bitStreams[i].Seek(0, SeekOrigin.Begin);
            }

            // Автодетект выравнивания
            if(inputStream.Length % NBITS != 0)
            {
                // Читаем данные из битовых потоков...
                int[] bitmap = _bitMaps.GetNextBitmap();
                for(int i = 0; i < NBITS; i++)
                {
                    bytesIn[bitmap[i]] = (byte)_bitStreams[i].ReadByte();
                }
                //...восстанавливаем порядок бит...
                Split8Bytes(bytesIn, bytesOut); // Bit-splitting...

                //...и пишем данные на свои места...
                //...параллельно "сливаем отстой" генератора случайных чисел записывая лишь align8
                outputStream.Write(bytesOut, 0, align8);

                // Учитываем обработанный объем
                remaining -= NBITS;
            }

            // Рассчитываем количество итераций на один процент
            long nItersTotal = remaining / NBITS;
            long itersForPercent = nItersTotal / 100;
            itersForPercent = (itersForPercent != 0) ? itersForPercent : 1; // Минимум - 1 итерация на процент!

            long nIter = 0;

            // Пока есть объем для обработки...
            while(remaining > 0)
            {
                // Читаем данные из битовых потоков...
                int[] bitmap = _bitMaps.GetNextBitmap();
                for(int i = 0; i < NBITS; i++)
                {
                    bytesIn[bitmap[i]] = (byte)_bitStreams[i].ReadByte();
                }
                //...восстанавливаем порядок бит...
                Split8Bytes(bytesIn, bytesOut); // Bit-splitting...

                //...и пишем данные на свои места...
                outputStream.Write(bytesOut, 0, NBITS);

                // Учитываем обработанный объем
                remaining -= NBITS;

                // Выводим прогресс
                if(++nIter % itersForPercent == 0)
                {
                    if(msg != null) msg.Progress("UnsplitFromBitstream (2/2)", (nIter / (double)nItersTotal) * 100, true);
                }
            }

            // Чистим данные...
            CryforceUtilities.ClearArray(bytesIn);
            CryforceUtilities.ClearArray(bytesOut);

            // Устанавливаем потоки на начальные позиции...
            inputStream.Seek(0, SeekOrigin.Begin);
            outputStream.Seek(0, SeekOrigin.Begin);

            // Синхронизируем буфер с физическим носителем...
            outputStream.Flush();

            // Вывод прогресса...
            if(msg != null) msg.Progress("BitSplitter", 100);
        }

        /// <summary>
        /// Очистка конфиденциальных данных
        /// </summary>
        public void Clear()
        {
            Clear(RndSeed, ZeroOut);
        }

        /// <summary>
        /// Очистка конфиденциальных данных
        /// </summary>
        /// <param name="rndSeed"> Инициализирующее значение генератора случайных чисел. </param>
        /// <param name="zeroOut"> Затирать выходной поток нулями? </param>
        public void Clear(int rndSeed, bool zeroOut)
        {
            // Производим стирание данных потоков, чтобы было невозможным восстановление) при помощи программных средств
            foreach(Stream bitStream in _bitStreams)
            {
                CryforceUtilities.WipeStream(msg, bitStream, BufferSizePerStream, 0, bitStream.Length, zeroOut, rndSeed);
            }
        }

        /// <summary>
        /// Очистка конфиденциальных данных (с закрытием потоков и удалением временных файлов)
        /// </summary>
        public void ClearAndClose()
        {
            // Указываем на деинициализацию
            IsInitialized = false;

            // Производим стирание данных потоков, чтобы было невозможным восстановление) при помощи программных средств
            foreach(Stream bitStream in _bitStreams)
            {
                CryforceUtilities.WipeStream(msg, bitStream, BufferSizePerStream, 0, bitStream.Length, ZeroOut, RndSeed);
                bitStream.Flush();
                bitStream.Close();
            }

            // Производим удаление носителей
            foreach(string bitStreamsName in BitStreamsNames)
            {
                if(File.Exists(bitStreamsName))
                {
                    File.SetAttributes(bitStreamsName, FileAttributes.Normal);
                    File.Delete(bitStreamsName);
                }
            }

            _bitMaps.ClearKey();
        }

        #endregion Public
    }
}