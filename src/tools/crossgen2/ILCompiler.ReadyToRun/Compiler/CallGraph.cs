// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Internal.TypeSystem;
using ILCompiler.DependencyAnalysis.ReadyToRun;


namespace ILCompiler
{
    public class CallGraphNode
    {
        public CallGraphNode(MethodWithGCInfo method)
        {
            Method = method;
        }

        public void AddCallee(CallGraphNode callee)
        {
            if (Callees == null)
            {
                Callees = new HashSet<CallGraphNode>();
            }

            Callees.Add(callee);
        }

        public MethodWithGCInfo Method { get; private set; }

        public bool IsDiscovered { get; set; }
        public bool IsCompleted { get; set; }

        public HashSet<CallGraphNode> Callees { get; set; }
    }

    public class CallGraph
    {
        HashSet<CallGraphNode> _rootNodes;
        HashSet<CallGraphNode> _nodes;
        Dictionary<MethodWithGCInfo, CallGraphNode> _methodToCallGraphNode;
        bool _computedOrder;

        public CallGraph()
        {
            _rootNodes = new HashSet<CallGraphNode>();
            _nodes = new HashSet<CallGraphNode>();
            _methodToCallGraphNode = new Dictionary<MethodWithGCInfo, CallGraphNode>();

        }

        public CallGraphNode AddNode(MethodWithGCInfo method)
        {
            CallGraphNode result;
            if (!_methodToCallGraphNode.TryGetValue(method, out result))
            {
                result = new CallGraphNode(method);
                _methodToCallGraphNode[method] = result;
                _nodes.Add(result);
            }
            return result;
        }

        public void AddRootNode(MethodWithGCInfo method)
        {
            CallGraphNode node = AddNode(method);
            _rootNodes.Add(node);
        }

        public void AddGraphEdge(MethodWithGCInfo caller, MethodWithGCInfo callee)
        {
            //Console.WriteLine("Adding call graph edge from {0} to {1}", caller.ToString(), callee.ToString());
            CallGraphNode calleeNode = AddNode(callee);
            _methodToCallGraphNode[caller].AddCallee(calleeNode);
        }

        public IEnumerable<CallGraphNode> GetPostOrder()
        {
            if (_computedOrder)
            {
                foreach (CallGraphNode node in _nodes)
                {
                    node.IsDiscovered = false;
                    node.IsCompleted = false;
                }
            }

            Stack<CallGraphNode> stack = new Stack<CallGraphNode>(_nodes.Count);
            List<CallGraphNode> result = new List<CallGraphNode>();
            foreach(CallGraphNode root in _nodes)
            {
                if (root.IsCompleted)
                {
                    continue;
                }
                else
                {
                    Debug.Assert(!root.IsDiscovered);
                    stack.Push(root);
                    root.IsDiscovered = true;
                }

                while (stack.Count != 0)
                {
                    CallGraphNode node = stack.Peek();
                    bool pushedSuccessor = false;
                    if (node.Callees != null)
                    {
                        foreach (CallGraphNode successor in node.Callees)
                        {
                            if (!successor.IsDiscovered)
                            {
                                stack.Push(successor);
                                successor.IsDiscovered = true;
                                pushedSuccessor = true;
                            }
                        }
                    }

                    if (!pushedSuccessor)
                    {
                        stack.Pop();
                        result.Add(node);
                        node.IsCompleted = true;
                    }
                }
            }

            _computedOrder = true;
            return result;
        }
    }
}
