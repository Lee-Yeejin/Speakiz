using System;
using System.IO;
using UnityEngine;

public static class WavUtility_
{
    // WAV 파일을 AudioClip으로 변환
    public static AudioClip ToAudioClip(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("파일을 찾을 수 없습니다: " + filePath);
            return null;
        }

        byte[] wavFile = File.ReadAllBytes(filePath);
        return FromWavData(wavFile);
    }

    // WAV 데이터에서 AudioClip 생성
    public static AudioClip FromWavData(byte[] wavFile)
    {
        // WAV 헤더 정보 추출
        int channels = BitConverter.ToInt16(wavFile, 22);
        int sampleRate = BitConverter.ToInt32(wavFile, 24);
        int subchunk2 = BitConverter.ToInt32(wavFile, 40);

        // 오디오 데이터 추출
        int startIndex = 44; // 오디오 데이터 시작 위치
        int samples = subchunk2 / 2; // 16-bit 오디오 데이터
        float[] audioData = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            short sample = BitConverter.ToInt16(wavFile, startIndex + i * 2);
            audioData[i] = sample / 32768.0f; // 16-bit 데이터를 float으로 변환
        }

        // AudioClip 생성
        AudioClip audioClip = AudioClip.Create("wavClip", samples, channels, sampleRate, false);
        audioClip.SetData(audioData, 0);
        return audioClip;
    }

    // AudioClip을 WAV 데이터로 변환 (녹음 저장용)
    public static byte[] FromAudioClip(AudioClip clip)
    {
        MemoryStream stream = new MemoryStream();

        // WAV 파일 헤더 생성
        const int headerSize = 44;
        int fileSize = clip.samples * clip.channels * 2 + headerSize;
        WriteHeader(stream, clip, fileSize);

        // 오디오 데이터를 바이트 배열로 변환
        float[] audioData = new float[clip.samples * clip.channels];
        clip.GetData(audioData, 0);
        short[] intData = new short[audioData.Length];

        // float ([-1.0f, 1.0f])을 short ([-32767, 32767])로 변환
        byte[] bytesData = new byte[audioData.Length * 2];
        int rescaleFactor = 32767; // float -> short 변환 비율

        for (int i = 0; i < audioData.Length; i++)
        {
            intData[i] = (short)(audioData[i] * rescaleFactor);
            byte[] byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        stream.Write(bytesData, 0, bytesData.Length);
        return stream.ToArray();
    }

    // WAV 헤더 쓰기
    private static void WriteHeader(Stream stream, AudioClip clip, int fileSize)
    {
        int hz = clip.frequency;
        int channels = clip.channels;
        int samples = clip.samples;

        stream.Seek(0, SeekOrigin.Begin);

        // RIFF 헤더
        byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        stream.Write(riff, 0, 4);

        byte[] chunkSize = BitConverter.GetBytes(fileSize - 8);
        stream.Write(chunkSize, 0, 4);

        byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        stream.Write(wave, 0, 4);

        // fmt 서브 청크
        byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        stream.Write(fmt, 0, 4);

        byte[] subChunk1 = BitConverter.GetBytes(16);
        stream.Write(subChunk1, 0, 4);

        ushort audioFormat = 1;
        byte[] audioFormatBytes = BitConverter.GetBytes(audioFormat);
        stream.Write(audioFormatBytes, 0, 2);

        byte[] numChannels = BitConverter.GetBytes(channels);
        stream.Write(numChannels, 0, 2);

        byte[] sampleRate = BitConverter.GetBytes(hz);
        stream.Write(sampleRate, 0, 4);

        byte[] byteRate = BitConverter.GetBytes(hz * channels * 2);
        stream.Write(byteRate, 0, 4);

        ushort blockAlign = (ushort)(channels * 2);
        stream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        ushort bps = 16;
        byte[] bitsPerSample = BitConverter.GetBytes(bps);
        stream.Write(bitsPerSample, 0, 2);

        // data 서브 청크
        byte[] dataString = System.Text.Encoding.UTF8.GetBytes("data");
        stream.Write(dataString, 0, 4);

        byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
        stream.Write(subChunk2, 0, 4);
    }
}
