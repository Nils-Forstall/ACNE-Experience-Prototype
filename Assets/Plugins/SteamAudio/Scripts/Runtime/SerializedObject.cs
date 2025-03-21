﻿//
// Copyright 2017-2023 Valve Corporation.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#if STEAMAUDIO_ENABLED

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SteamAudio
{
    public class SerializedObject
    {
        IntPtr mSerializedObject = IntPtr.Zero;
        IntPtr mDataBuffer = IntPtr.Zero;

        static List<SerializedData> sAssetsToFlush = null;

        public SerializedObject(Context context)
        {
            var serializedObjectSettings = new SerializedObjectSettings { };

            API.iplSerializedObjectCreate(context.Get(), ref serializedObjectSettings, out mSerializedObject);
        }

        public SerializedObject(Context context, SerializedData dataAsset)
        {
            var data = dataAsset.data;
            mDataBuffer = Marshal.AllocHGlobal(data.Length);

            Marshal.Copy(data, 0, mDataBuffer, data.Length);

            var serializedObjectSettings = new SerializedObjectSettings { };
            serializedObjectSettings.data = mDataBuffer;
            serializedObjectSettings.size = (UIntPtr) data.Length;

            API.iplSerializedObjectCreate(context.Get(), ref serializedObjectSettings, out mSerializedObject);
        }

        public SerializedObject(SerializedObject serializedObject)
        {
            mSerializedObject = API.iplSerializedObjectRetain(serializedObject.Get());
        }

        ~SerializedObject()
        {
            Release();
        }

        public void Release()
        {
            if (mDataBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(mDataBuffer);
                mDataBuffer = IntPtr.Zero;
            }

            API.iplSerializedObjectRelease(ref mSerializedObject);
        }

        public IntPtr Get()
        {
            return mSerializedObject;
        }

        public UIntPtr GetSize()
        {
            return API.iplSerializedObjectGetSize(mSerializedObject);
        }

        public IntPtr GetData()
        {
            return API.iplSerializedObjectGetData(mSerializedObject);
        }

        public void WriteToFile(SerializedData dataAsset, bool flush = true)
        {
            var dataSize = GetSize();
            var dataBuffer = GetData();

            dataAsset.data = new byte[(int) dataSize];
            Marshal.Copy(dataBuffer, dataAsset.data, 0, (int) dataSize);

            if (flush)
            {
                FlushWrite(dataAsset);
            }
            else
            {
                if (sAssetsToFlush == null)
                {
                    sAssetsToFlush = new List<SerializedData>();
                }

                sAssetsToFlush.Add(dataAsset);
            }
        }

        public static void FlushWrite(SerializedData dataAsset)
        {
#if UNITY_EDITOR
            var assetPaths = new string[1];
            assetPaths[0] = AssetDatabase.GetAssetPath(dataAsset);

            // TODO: Deprecate older versions of Unity.
#if UNITY_2017_3_OR_NEWER
            AssetDatabase.ForceReserializeAssets(assetPaths);
#endif
#endif
        }

        public static void FlushAllWrites()
        {
            if (sAssetsToFlush == null)
                return;

            foreach (var dataAsset in sAssetsToFlush)
            {
                FlushWrite(dataAsset);
            }

            sAssetsToFlush.Clear();
        }
    }
}
#endif
