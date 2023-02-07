﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.GoCsp
{
    public class Map<TKey, TValue>
    {
        internal enum RBColor
        {
            Red,
            Black
        }

        int _count;
        readonly bool _multi;
        readonly MapNode<TKey, TValue> _head;

        public Map(bool multi = false)
        {
            _count = 0;
            _multi = multi;
            _head = new MapNode<TKey, TValue>(true);
            _head.color = RBColor.Black;
            root = lmost = rmost = _head;
        }

        static public MapNode<TKey, TValue> NewNode(TKey key, TValue value)
        {
            return new MapNode<TKey, TValue>(false) { key = key, value = value, color = RBColor.Red };
        }

        public MapNode<TKey, TValue> ReNewNode(MapNode<TKey, TValue> oldNode, TKey key, TValue value)
        {
            if (!oldNode.Isolated)
            {
                Remove(oldNode);
            }
            oldNode.key = key;
            oldNode.value = value;
            oldNode.color = RBColor.Red;
            return oldNode;
        }

        static bool comp_lt<T>(T x, T y)
        {
            return Comparer<T>.Default.Compare(x, y) < 0;
        }

        static bool is_nil(MapNode<TKey, TValue> node)
        {
            return node.nil;
        }

        MapNode<TKey, TValue> root
        {
            get
            {
                return _head.parent;
            }
            set
            {
                _head.parent = value;
            }
        }

        MapNode<TKey, TValue> lmost
        {
            get
            {
                return _head.left;
            }
            set
            {
                _head.left = value;
            }
        }

        MapNode<TKey, TValue> rmost
        {
            get
            {
                return _head.right;
            }
            set
            {
                _head.right = value;
            }
        }

        void left_rotate(MapNode<TKey, TValue> whereNode)
        {
            MapNode<TKey, TValue> pNode = whereNode.right;
            whereNode.right = pNode.left;
            if (!is_nil(pNode.left))
            {
                pNode.left.parent = whereNode;
            }
            pNode.parent = whereNode.parent;
            if (whereNode == root)
            {
                root = pNode;
            }
            else if (whereNode == whereNode.parent.left)
            {
                whereNode.parent.left = pNode;
            }
            else
            {
                whereNode.parent.right = pNode;
            }
            pNode.left = whereNode;
            whereNode.parent = pNode;
        }

        void right_rotate(MapNode<TKey, TValue> whereNode)
        {
            MapNode<TKey, TValue> pNode = whereNode.left;
            whereNode.left = pNode.right;
            if (!is_nil(pNode.right))
            {
                pNode.right.parent = whereNode;
            }
            pNode.parent = whereNode.parent;
            if (whereNode == root)
            {
                root = pNode;
            }
            else if (whereNode == whereNode.parent.right)
            {
                whereNode.parent.right = pNode;
            }
            else
            {
                whereNode.parent.left = pNode;
            }
            pNode.right = whereNode;
            whereNode.parent = pNode;
        }

        void insert_at(bool addLeft, MapNode<TKey, TValue> whereNode, MapNode<TKey, TValue> newNode)
        {
            newNode.parent = whereNode;
            if (whereNode == _head)
            {
                root = lmost = rmost = newNode;
            }
            else if (addLeft)
            {
                whereNode.left = newNode;
                if (whereNode == lmost)
                {
                    lmost = newNode;
                }
            }
            else
            {
                whereNode.right = newNode;
                if (whereNode == rmost)
                {
                    rmost = newNode;
                }
            }
            for (MapNode<TKey, TValue> pNode = newNode; RBColor.Red == pNode.parent.color;)
            {
                if (pNode.parent == pNode.parent.parent.left)
                {
                    whereNode = pNode.parent.parent.right;
                    if (RBColor.Red == whereNode.color)
                    {
                        pNode.parent.color = RBColor.Black;
                        whereNode.color = RBColor.Black;
                        pNode.parent.parent.color = RBColor.Red;
                        pNode = pNode.parent.parent;
                    }
                    else
                    {
                        if (pNode == pNode.parent.right)
                        {
                            pNode = pNode.parent;
                            left_rotate(pNode);
                        }
                        pNode.parent.color = RBColor.Black;
                        pNode.parent.parent.color = RBColor.Red;
                        right_rotate(pNode.parent.parent);
                    }
                }
                else
                {
                    whereNode = pNode.parent.parent.left;
                    if (RBColor.Red == whereNode.color)
                    {
                        pNode.parent.color = RBColor.Black;
                        whereNode.color = RBColor.Black;
                        pNode.parent.parent.color = RBColor.Red;
                        pNode = pNode.parent.parent;
                    }
                    else
                    {
                        if (pNode == pNode.parent.left)
                        {
                            pNode = pNode.parent;
                            right_rotate(pNode);
                        }
                        pNode.parent.color = RBColor.Black;
                        pNode.parent.parent.color = RBColor.Red;
                        left_rotate(pNode.parent.parent);
                    }
                }
            }
            root.color = RBColor.Black;
            _count++;
        }

        void insert(MapNode<TKey, TValue> newNode, bool priorityRight)
        {
            newNode.parent = newNode.left = newNode.right = _head;
            MapNode<TKey, TValue> tryNode = root;
            MapNode<TKey, TValue> whereNode = _head;
            bool addLeft = true;
            while (!is_nil(tryNode))
            {
                whereNode = tryNode;
                addLeft = priorityRight ? comp_lt(newNode.key, tryNode.key) : !comp_lt(tryNode.key, newNode.key);
                tryNode = addLeft ? tryNode.left : tryNode.right;
            }
            if (_multi)
            {
                insert_at(addLeft, whereNode, newNode);
            }
            else
            {
                MapNode<TKey, TValue> where = whereNode;
                if (!addLeft) { }
                else if (where == lmost)
                {
                    insert_at(true, whereNode, newNode);
                    return;
                }
                else
                {
                    where = previous(where);
                }
                if (comp_lt(where.key, newNode.key))
                {
                    insert_at(addLeft, whereNode, newNode);
                }
                else
                {
                    newNode.parent = newNode.left = newNode.right = null;
                }
            }
        }

        MapNode<TKey, TValue> new_inter_node(TKey key, TValue value)
        {
            MapNode<TKey, TValue> newNode = NewNode(key, value);
            newNode.parent = newNode.left = newNode.right = _head;
            return newNode;
        }

        MapNode<TKey, TValue> insert(TKey key, TValue value, bool priorityRight)
        {
            MapNode<TKey, TValue> tryNode = root;
            MapNode<TKey, TValue> whereNode = _head;
            bool addLeft = true;
            while (!is_nil(tryNode))
            {
                whereNode = tryNode;
                addLeft = priorityRight ? comp_lt(key, tryNode.key) : !comp_lt(tryNode.key, key);
                tryNode = addLeft ? tryNode.left : tryNode.right;
            }
            MapNode<TKey, TValue> newNode = null;
            if (_multi)
            {
                newNode = new_inter_node(key, value);
                insert_at(addLeft, whereNode, newNode);
            }
            else
            {
                MapNode<TKey, TValue> where = whereNode;
                if (!addLeft) { }
                else if (where == lmost)
                {
                    newNode = new_inter_node(key, value);
                    insert_at(true, whereNode, newNode);
                    return newNode;
                }
                else
                {
                    where = previous(where);
                }
                if (comp_lt(where.key, key))
                {
                    newNode = new_inter_node(key, value);
                    insert_at(addLeft, whereNode, newNode);
                }
            }
            return newNode;
        }

        void remove(MapNode<TKey, TValue> where)
        {
            MapNode<TKey, TValue> erasedNode = where;
            where = next(where);
            MapNode<TKey, TValue> fixNode = null;
            MapNode<TKey, TValue> fixNodeParent = null;
            MapNode<TKey, TValue> pNode = erasedNode;
            if (is_nil(pNode.left))
            {
                fixNode = pNode.right;
            }
            else if (is_nil(pNode.right))
            {
                fixNode = pNode.left;
            }
            else
            {
                pNode = where;
                fixNode = pNode.right;
            }
            if (pNode == erasedNode)
            {
                fixNodeParent = erasedNode.parent;
                if (!is_nil(fixNode))
                {
                    fixNode.parent = fixNodeParent;
                }
                if (root == erasedNode)
                {
                    root = fixNode;
                }
                else if (fixNodeParent.left == erasedNode)
                {
                    fixNodeParent.left = fixNode;
                }
                else
                {
                    fixNodeParent.right = fixNode;
                }
                if (lmost == erasedNode)
                {
                    lmost = is_nil(fixNode) ? fixNodeParent : min(fixNode);
                }
                if (rmost == erasedNode)
                {
                    rmost = is_nil(fixNode) ? fixNodeParent : max(fixNode);
                }
            }
            else
            {
                erasedNode.left.parent = pNode;
                pNode.left = erasedNode.left;
                if (pNode == erasedNode.right)
                {
                    fixNodeParent = pNode;
                }
                else
                {
                    fixNodeParent = pNode.parent;
                    if (!is_nil(fixNode))
                    {
                        fixNode.parent = fixNodeParent;
                    }
                    fixNodeParent.left = fixNode;
                    pNode.right = erasedNode.right;
                    erasedNode.right.parent = pNode;
                }
                if (root == erasedNode)
                {
                    root = pNode;
                }
                else if (erasedNode.parent.left == erasedNode)
                {
                    erasedNode.parent.left = pNode;
                }
                else
                {
                    erasedNode.parent.right = pNode;
                }
                pNode.parent = erasedNode.parent;
                RBColor tcol = pNode.color;
                pNode.color = erasedNode.color;
                erasedNode.color = tcol;
            }
            if (RBColor.Black == erasedNode.color)
            {
                for (; fixNode != root && RBColor.Black == fixNode.color; fixNodeParent = fixNode.parent)
                {
                    if (fixNode == fixNodeParent.left)
                    {
                        pNode = fixNodeParent.right;
                        if (RBColor.Red == pNode.color)
                        {
                            pNode.color = RBColor.Black;
                            fixNodeParent.color = RBColor.Red;
                            left_rotate(fixNodeParent);
                            pNode = fixNodeParent.right;
                        }
                        if (is_nil(pNode))
                        {
                            fixNode = fixNodeParent;
                        }
                        else if (RBColor.Black == pNode.left.color && RBColor.Black == pNode.right.color)
                        {
                            pNode.color = RBColor.Red;
                            fixNode = fixNodeParent;
                        }
                        else
                        {
                            if (RBColor.Black == pNode.right.color)
                            {
                                pNode.left.color = RBColor.Black;
                                pNode.color = RBColor.Red;
                                right_rotate(pNode);
                                pNode = fixNodeParent.right;
                            }
                            pNode.color = fixNodeParent.color;
                            fixNodeParent.color = RBColor.Black;
                            pNode.right.color = RBColor.Black;
                            left_rotate(fixNodeParent);
                            break;
                        }
                    }
                    else
                    {
                        pNode = fixNodeParent.left;
                        if (RBColor.Red == pNode.color)
                        {
                            pNode.color = RBColor.Black;
                            fixNodeParent.color = RBColor.Red;
                            right_rotate(fixNodeParent);
                            pNode = fixNodeParent.left;
                        }
                        if (is_nil(pNode))
                        {
                            fixNode = fixNodeParent;
                        }
                        else if (RBColor.Black == pNode.right.color && RBColor.Black == pNode.left.color)
                        {
                            pNode.color = RBColor.Red;
                            fixNode = fixNodeParent;
                        }
                        else
                        {
                            if (RBColor.Black == pNode.left.color)
                            {
                                pNode.right.color = RBColor.Black;
                                pNode.color = RBColor.Red;
                                left_rotate(pNode);
                                pNode = fixNodeParent.left;
                            }
                            pNode.color = fixNodeParent.color;
                            fixNodeParent.color = RBColor.Black;
                            pNode.left.color = RBColor.Black;
                            right_rotate(fixNodeParent);
                            break;
                        }
                    }
                }
                fixNode.color = RBColor.Black;
            }
            erasedNode.parent = erasedNode.left = erasedNode.right = null;
            _count--;
        }

        MapNode<TKey, TValue> lbound(TKey key)
        {
            MapNode<TKey, TValue> pNode = root;
            MapNode<TKey, TValue> whereNode = _head;
            while (!is_nil(pNode))
            {
                if (comp_lt(pNode.key, key))
                {
                    pNode = pNode.right;
                }
                else
                {
                    whereNode = pNode;
                    pNode = pNode.left;
                }
            }
            return whereNode;
        }

        MapNode<TKey, TValue> rbound(TKey key)
        {
            MapNode<TKey, TValue> pNode = root;
            MapNode<TKey, TValue> whereNode = _head;
            while (!is_nil(pNode))
            {
                if (comp_lt(key, pNode.key))
                {
                    pNode = pNode.left;
                }
                else
                {
                    whereNode = pNode;
                    pNode = pNode.right;
                }
            }
            return whereNode;
        }

        Tuple<bool, MapNode<TKey, TValue>> lbound_insert(TKey key)
        {
            MapNode<TKey, TValue> pNode = root;
            MapNode<TKey, TValue> insertWhereNode = _head;
            MapNode<TKey, TValue> boundWhereNode = _head;
            bool addLeft = true;
            while (!is_nil(pNode))
            {
                insertWhereNode = pNode;
                if (comp_lt(pNode.key, key))
                {
                    addLeft = false;
                    pNode = pNode.right;
                }
                else
                {
                    addLeft = true;
                    boundWhereNode = pNode;
                    pNode = pNode.left;
                }
            }
            if (!is_nil(boundWhereNode) && !comp_lt(key, boundWhereNode.key))
            {
                return tuple.make(true, boundWhereNode);
            }
            MapNode<TKey, TValue> newNode = new_inter_node(key, default(TValue));
            insert_at(addLeft, insertWhereNode, newNode);
            return tuple.make(false, newNode);
        }

        Tuple<bool, MapNode<TKey, TValue>> rbound_insert(TKey key)
        {
            MapNode<TKey, TValue> pNode = root;
            MapNode<TKey, TValue> insertWhereNode = _head;
            MapNode<TKey, TValue> boundWhereNode = _head;
            bool addLeft = true;
            while (!is_nil(pNode))
            {
                insertWhereNode = pNode;
                if (comp_lt(key, pNode.key))
                {
                    addLeft = false;
                    pNode = pNode.left;
                }
                else
                {
                    addLeft = true;
                    boundWhereNode = pNode;
                    pNode = pNode.right;
                }
            }
            if (!is_nil(boundWhereNode) && !comp_lt(boundWhereNode.key, key))
            {
                return tuple.make(true, boundWhereNode);
            }
            MapNode<TKey, TValue> newNode = new_inter_node(key, default(TValue));
            insert_at(addLeft, insertWhereNode, newNode);
            return tuple.make(false, newNode);
        }

        static MapNode<TKey, TValue> max(MapNode<TKey, TValue> pNode)
        {
            while (!is_nil(pNode.right))
            {
                pNode = pNode.right;
            }
            return pNode;
        }

        static MapNode<TKey, TValue> min(MapNode<TKey, TValue> pNode)
        {
            while (!is_nil(pNode.left))
            {
                pNode = pNode.left;
            }
            return pNode;
        }

        static internal MapNode<TKey, TValue> next(MapNode<TKey, TValue> ptr)
        {
            if (is_nil(ptr))
            {
                return ptr;
            }
            else if (!is_nil(ptr.right))
            {
                return min(ptr.right);
            }
            else
            {
                MapNode<TKey, TValue> pNode;
                while (!is_nil(pNode = ptr.parent) && ptr == pNode.right)
                {
                    ptr = pNode;
                }
                return pNode;
            }
        }

        static internal MapNode<TKey, TValue> previous(MapNode<TKey, TValue> ptr)
        {
            if (is_nil(ptr))
            {
                return ptr;
            }
            else if (!is_nil(ptr.left))
            {
                return max(ptr.left);
            }
            else
            {
                MapNode<TKey, TValue> pNode;
                while (!is_nil(pNode = ptr.parent) && ptr == pNode.left)
                {
                    ptr = pNode;
                }
                return pNode;
            }
        }

        static void erase(MapNode<TKey, TValue> rootNode)
        {
            for (MapNode<TKey, TValue> pNode = rootNode; !is_nil(pNode); rootNode = pNode)
            {
                erase(pNode.right);
                pNode = pNode.left;
                rootNode.parent = rootNode.left = rootNode.right = null;
            }
        }

        public int Count
        {
            get
            {
                return _count;
            }
        }

        public void Clear()
        {
            erase(root);
            root = lmost = rmost = _head;
            _count = 0;
        }

        public bool Has(TKey key)
        {
            MapNode<TKey, TValue> node = lbound(key);
            return !is_nil(node) && !comp_lt(key, node.key);
        }

        public MapNode<TKey, TValue> FindFirstGE(TKey key)
        {
            MapNode<TKey, TValue> node = lbound(key);
            return !is_nil(node) ? node : null;
        }

        public MapNode<TKey, TValue> FindFirstLE(TKey key)
        {
            MapNode<TKey, TValue> node = rbound(key);
            return !is_nil(node) ? node : null;
        }

        public MapNode<TKey, TValue> FindFirst(TKey key)
        {
            MapNode<TKey, TValue> node = lbound(key);
            return !is_nil(node) && !comp_lt(key, node.key) ? node : null;
        }

        public MapNode<TKey, TValue> FindLast(TKey key)
        {
            MapNode<TKey, TValue> node = rbound(key);
            return !is_nil(node) && !comp_lt(node.key, key) ? node : null;
        }

        public MapNode<TKey, TValue> FindRight(TKey key)
        {
            MapNode<TKey, TValue> node = lbound(key);
            return is_nil(node) ? null : node;
        }

        public MapNode<TKey, TValue> FindLeft(TKey key)
        {
            MapNode<TKey, TValue> node = rbound(key);
            return is_nil(node) ? null : node;
        }

        public Tuple<bool, MapNode<TKey, TValue>> GetFirst(TKey key)
        {
            return lbound_insert(key);
        }

        public Tuple<bool, MapNode<TKey, TValue>> GetLast(TKey key)
        {
            return rbound_insert(key);
        }

        public void Remove(MapNode<TKey, TValue> node)
        {
            remove(node);
        }

        public MapNode<TKey, TValue> Insert(TKey key, TValue value, bool priorityRight = true)
        {
            return insert(key, value, _multi ? priorityRight : true);
        }

        public bool Insert(MapNode<TKey, TValue> newNode, bool priorityRight = true)
        {
            insert(newNode, _multi ? priorityRight : true);
            return !newNode.Isolated;
        }

        public IEnumerator<Tuple<TKey, TValue>> GetEnumerator()
        {
            MapNode<TKey, TValue> it = First;
            while (null != it)
            {
                yield return Tuple.make(it.key, it.value);
                it = it.Next;
            }
            yield break;
        }

        public Tuple<TKey, TValue>[] ToList
        {
            get
            {
                int idx = 0;
                Tuple<TKey, TValue>[] list = new Tuple<TKey, TValue>[_count];
                MapNode<TKey, TValue> it = First;
                while (null != it)
                {
                    list[idx++] = tuple.make(it.key, it.value);
                    it = it.Next;
                }
                return list;
            }
        }

        public MapNode<TKey, TValue> First
        {
            get
            {
                return is_nil(lmost) ? null : lmost;
            }
        }

        public MapNode<TKey, TValue> Last
        {
            get
            {
                return is_nil(rmost) ? null : rmost;
            }
        }
    }

    public class MapNode<TKey, TValue>
    {
        internal TKey key;
        internal TValue value;
        internal MapNode<TKey, TValue> parent;
        internal MapNode<TKey, TValue> left;
        internal MapNode<TKey, TValue> right;
        internal Map<TKey, TValue>.RBColor color;
        internal readonly bool nil;

        internal MapNode(bool n)
        {
            nil = n;
        }

        public MapNode<TKey, TValue> Next
        {
            get
            {
                MapNode<TKey, TValue> pNode = Map<TKey, TValue>.next(this);
                return pNode.nil ? null : pNode;
            }
        }

        public MapNode<TKey, TValue> Prev
        {
            get
            {
                MapNode<TKey, TValue> pNode = Map<TKey, TValue>.previous(this);
                return pNode.nil ? null : pNode;
            }
        }

        public TKey Key
        {
            get
            {
                return key;
            }
        }

        public TValue Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        public bool Isolated
        {
            get
            {
                return null == parent;
            }
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", key, value);
        }
    }
}
