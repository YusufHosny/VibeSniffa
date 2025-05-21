// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace VibeSniffa
{
    public class BoundsInference
    {
        private float[] m_topLeft, m_bottomRight;

        private BoundsInference() { }

        public static List<BoundsInference> FromJsonString(string jsonRaw)
        {
            var jsonObject = JObject.Parse(jsonRaw);

            var bounds = (JArray)jsonObject["bounds"];

            var inferences = new List<BoundsInference>();
            for (var i = 0; i < bounds.Count; i++)
            {
                var bound = (JArray)bounds[i];

                var inference = new BoundsInference
                {
                    m_topLeft = new float[] { (float)bound[0], (float)bound[1] },
                    m_bottomRight = new float[] { (float)bound[2], (float)bound[3] }
                };

                inferences.Add(inference);
            }

            return inferences;
        }

        public (float, float) GetTopLeft() => (m_topLeft[0], m_topLeft[1]);
        public (float, float) GetBottomRight() => (m_bottomRight[0], m_bottomRight[1]);

        public float GetCenterX() => (m_topLeft[0] + m_bottomRight[0]) / 2;
        public float GetCenterY() => (m_topLeft[1] + m_bottomRight[1]) / 2;
        public (float, float) GetCenter() => (GetCenterX(), GetCenterY());


        public float GetWidth() => Math.Abs(m_topLeft[0] - m_bottomRight[0]);
        public float GetHeight() => Math.Abs(m_topLeft[1] - m_bottomRight[1]);
        public (float, float) GetDimensions() => (GetWidth(), GetHeight());

        public override string ToString()
        {
            return $"bounds(tl:{m_topLeft}, br:{m_bottomRight})";
        }

    }

    public class EmotionInference
    {

        public string Emotion { get; set; }
        public string Description { get; set; }

        private EmotionInference() { }

        public static EmotionInference FromJsonString(string jsonRaw)
        {
            var jsonObject = JObject.Parse(jsonRaw);

            var inference = new EmotionInference
            {
                Emotion = (string)jsonObject["emotion"],
                Description = (string)jsonObject["description"]
            };

            return inference;
        }

        public override string ToString()
        {
            return $"emotion({Emotion}: {Description})";
        }


    }
}
