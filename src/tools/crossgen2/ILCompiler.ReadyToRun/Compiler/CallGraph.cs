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
        public CallGraphNode(MethodDesc method)
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

        public MethodDesc Method { get; private set; }

        List<CallGraphNode> _callees;
    }

    public class CallGraph
    {
        HashSet<CallGraphNode> _rootNodes;
        HashSet<CallGraphNode> _nodes;
        Dictionary<MethodDesc, CallGraphNode> _methodDescToCallGraphNode;

        public CallGraph()
        {
            _rootNodes = new HashSet<CallGraphNode>();
            _nodes = new HashSet<CallGraphNode>();
            _methodDescToCallGraphNode = new Dictionary<MethodDesc, CallGraphNode>();

        }

        public CallGraphNode AddNode(MethodDesc method)
        {
            CallGraphNode result;
            if (!_methodDescToCallGraphNode.TryGetValue(method, out result))
            {
                result = new CallGraphNode(method);
                _methodDescToCallGraphNode[method] = result;
                _nodes.Add(result);
            }
            return result;
        }

        public void AddRootNode(MethodDesc method)
        {
            CallGraphNode node = AddNode(method);
            _rootNodes.Add(node);
        }

        public void AddGraphEdge(MethodDesc caller, MethodDesc callee)
        {
            Console.WriteLine("Adding call graph edge from {0} to {1}", caller.ToString(), callee.ToString());
            CallGraphNode calleeNode;
            if (!_methodDescToCallGraphNode.TryGetValue(callee, out calleeNode))
            {
                calleeNode = new CallGraphNode(callee);
            }
            _methodDescToCallGraphNode[caller].AddCallee(calleeNode);
        }

        public ICollection<CallGraphNode> GetNodes()
        {
            return _nodes;
        }
    }
}
