using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Ascension.Compiler
{
    /// <summary>
    ///     Asset Folder
    /// </summary>
    [ProtoContract]
    public class AssetFolder : INamedAsset
    {
        [ProtoMember(4)] public AssetDefinition[] Assets = new AssetDefinition[0];
        [ProtoIgnore] public bool Deleted;
        [ProtoMember(2)] public bool Expanded;
        [ProtoMember(3)] public AssetFolder[] Folders = new AssetFolder[0];
        [ProtoMember(5)] public Guid Guid;
        [ProtoMember(1)] public string Name;

        public IEnumerable<INamedAsset> Children
        {
            get
            {
                return
                    Folders.OrderBy(x => x.Name)
                        .Cast<INamedAsset>()
                        .Concat(Assets.OrderBy(x => x.Name).Cast<INamedAsset>());
            }
        }

        public IEnumerable<AssetDefinition> AssetsAll
        {
            get { return Assets.Concat(Folders.SelectMany(x => x.AssetsAll)); }
        }

        string INamedAsset.GetName()
        {
            return Name;
        }

        public INamedAsset FindFirstChild()
        {
            return Children.FirstOrDefault();
        }

        public AssetFolder FindParentFolder(INamedAsset asset)
        {
            if (Children.Contains(asset))
            {
                return this;
            }

            foreach (AssetFolder f in Folders)
            {
                AssetFolder parent = f.FindParentFolder(asset);

                if (parent != null)
                {
                    return parent;
                }
            }

            return null;
        }

        public INamedAsset FindPrevSibling(INamedAsset asset)
        {
            return FindSibling(asset, true);
        }

        public INamedAsset FindNextSibling(INamedAsset asset)
        {
            return FindSibling(asset, false);
        }

        private INamedAsset FindSibling(INamedAsset asset, bool prev)
        {
            if (Children.Contains(asset))
            {
                INamedAsset[] array = Children.ToArray();
                int index = Array.IndexOf(array, asset);

                if (prev)
                {
                    if (index == 0)
                    {
                        return null;
                    }

                    return array[index - 1];
                }
                if (index + 1 == array.Length)
                {
                    return null;
                }

                return array[index + 1];
            }

            foreach (AssetFolder f in Folders)
            {
                INamedAsset sibling = f.FindSibling(asset, prev);

                if (sibling != null)
                {
                    return sibling;
                }
            }

            return null;
        }
    }
}