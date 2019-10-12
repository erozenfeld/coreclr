// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

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
            if (_callees == null)
            {
                _callees = new List<CallGraphNode>();
            }

            _callees.Add(callee);
        }

        public MethodWithGCInfo Method { get; private set; }

        List<CallGraphNode> _callees;
    }

    public class CallGraph
    {
        HashSet<CallGraphNode> _rootNodes;
        HashSet<CallGraphNode> _nodes;
        Dictionary<MethodWithGCInfo, CallGraphNode> _methodToCallGraphNode;

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
            CallGraphNode calleeNode;
            if (!_methodToCallGraphNode.TryGetValue(callee, out calleeNode))
            {
                calleeNode = new CallGraphNode(callee);
            }
            _methodToCallGraphNode[caller].AddCallee(calleeNode);
        }

        public ICollection<CallGraphNode> GetNodes()
        {
            return _nodes;
        }
    }
}
