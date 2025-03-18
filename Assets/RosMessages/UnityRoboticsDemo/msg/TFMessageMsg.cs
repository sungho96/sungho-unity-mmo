//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.UnityRoboticsDemo
{
    [Serializable]
    public class TFMessageMsg : Message
    {
        public const string k_RosMessageName = "unity_robotics_demo_msgs/TFMessage";
        public override string RosMessageName => k_RosMessageName;

        public Geometry.TransformStampedMsg[] transforms;

        public TFMessageMsg()
        {
            this.transforms = new Geometry.TransformStampedMsg[0];
        }

        public TFMessageMsg(Geometry.TransformStampedMsg[] transforms)
        {
            this.transforms = transforms;
        }

        public static TFMessageMsg Deserialize(MessageDeserializer deserializer) => new TFMessageMsg(deserializer);

        private TFMessageMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.transforms, Geometry.TransformStampedMsg.Deserialize, deserializer.ReadLength());
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.WriteLength(this.transforms);
            serializer.Write(this.transforms);
        }

        public override string ToString()
        {
            return "TFMessageMsg: " +
            "\ntransforms: " + System.String.Join(", ", transforms.ToList());
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
