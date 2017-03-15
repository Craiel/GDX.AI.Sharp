namespace GDX.AI.Sharp.BTree.Utils
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    using Enums;

    using Exceptions;

    using Sharp.Utils;

    using YamlDotNet.RepresentationModel;

    /// <summary>
    /// Serializer for <see cref="BehaviorStream{T}"/>
    /// </summary>
    /// <typeparam name="T">the type of <see cref="IBlackboard"/> the tree is using</typeparam>
    public class BehaviorTreeSerializer<T>
        where T : IBlackboard
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------

        /// <summary>
        /// Serialize the given tree to string format using Json
        /// </summary>
        /// <param name="tree">the tree to serialize</param>
        /// <returns>the string data for the tree</returns>
        /// <exception cref="SerializationException">if any data is not able to serialize</exception>
        public string Serialize(BehaviorStream<T> tree)
        {
            var serializer = new YamlFluentSerializer();
            serializer.Begin(YamlContainerType.Dictionary)
                .Add("Size", tree.stream.Length)
                .Add("GrowBy", tree.GrowBy)
                .Add("RootId", tree.Root.Value)
            .End();

            serializer.Begin(YamlContainerType.Dictionary);
            for (var i = 0; i < tree.stream.Length; i++)
            {
                if (tree.stream[i] == null)
                {
                    serializer.Add(i);
                    continue;
                }

                serializer.Add(i, tree.stream[i].GetType().AssemblyQualifiedName);
            }

            serializer.End();

            serializer.Begin(YamlContainerType.Dictionary);
            for (var i = 0; i < tree.stream.Length; i++)
            {
                if (tree.stream[i] == null)
                {
                    serializer.Add(i);
                    continue;
                }

                serializer.Add(i, tree.stream[i]);
            }
            serializer.End();
            
            return serializer.Serialize();
        }

        /// <summary>
        /// Deserialize the given data into a <see cref="BehaviorStream{T}"/>.
        /// </summary>
        /// <param name="blackboard">the <see cref="IBlackboard"/> the deserialized tree will use</param>
        /// <param name="data">the data to load from</param>
        /// <returns>the deserialized tree</returns>
        /// <exception cref="SerializationException">if the data fails to deserialize</exception>
        public BehaviorStream<T> Deserialize(T blackboard, string data)
        {
            BehaviorTreeStorage storage;
            if (!GDXAI.Serializer.Deserialize(data, out storage))
            {
                throw new SerializationException("Failed to deserialize tree data");
            }

            BehaviorStream<T> result = new BehaviorStream<T>(blackboard, storage.Size, storage.GrowBy) { Root = storage.Root };
            foreach (int index in storage.TaskType.Keys)
            {
                string typeName = storage.TaskType[index];
                if (string.IsNullOrEmpty(typeName))
                {
                    continue;
                }

                Type taskType = Type.GetType(typeName);
                if (taskType == null)
                {
                    throw new SerializationException("Could not load tree, unknown type: " + typeName);
                }

                object taskObject;
                if (!GDXAI.Serializer.Deserialize(taskType, storage.TaskContent[index], out taskObject))
                {
                    throw new SerializationException("Failed to load Task");
                }

                result.stream[index] = (Task<T>)taskObject;
            }

            return result;
        }
    }
}
