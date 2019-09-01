﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

namespace LovettSoftware.SmartSockets
{
    public class SmartSocketTypeResolver : DataContractResolver
    {
        private readonly Dictionary<string, Type> typeMap = new Dictionary<string, Type>();

        public SmartSocketTypeResolver()
        {
            this.AddBaseTypes();
        }

        public SmartSocketTypeResolver(params Type[] knownTypes)
        {
            this.AddTypes(knownTypes);
        }

        public SmartSocketTypeResolver(IEnumerable<Type> knownTypes)
        {
            this.AddTypes(knownTypes);
        }

        private void AddTypes(IEnumerable<Type> knownTypes)
        {
            this.AddBaseTypes();
            foreach (var t in knownTypes)
            {
                this.typeMap[t.FullName] = t;
            }
        }

        private void AddBaseTypes()
        {
            foreach (var t in new Type[] { typeof(SocketMessage) })
            {
                this.typeMap[t.FullName] = t;
            }
        }

        public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
        {
            string fullName = typeName;
            if (!string.IsNullOrEmpty(typeNamespace))
            {
                Uri uri = new Uri(typeNamespace);
                string clrNamespace = uri.Segments.Last();
                fullName = clrNamespace + "." + typeName;
            }

            if (!this.typeMap.TryGetValue(fullName, out Type t))
            {
                t = knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
            }

            return t;
        }

        public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
        {
            return knownTypeResolver.TryResolveType(type, declaredType, knownTypeResolver, out typeName, out typeNamespace);
        }
    }
}
