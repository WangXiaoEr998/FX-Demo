using System;
using System.Collections.Generic;
using UnityEngine;

namespace Helper
{
    /// <summary>
    /// 稳定排序算法集合 - 保持相等元素的相对顺序
    /// 作者：容泳森
    /// 创建时间：2025-8-12
    /// </summary>
    public static class StableSortHelper
    {
        #region 基础工具方法

        /// <summary>
        /// 交换数组中的两个元素的位置
        /// </summary>
        private static void Swap<T>(T[] array, int indexX, int indexY)
        {
            (array[indexX], array[indexY]) = (array[indexY], array[indexX]);
        }

        /// <summary>
        /// 交换List中的两个元素的位置
        /// </summary>
        private static void Swap<T>(List<T> list, int indexX, int indexY)
        {
            (list[indexX], list[indexY]) = (list[indexY], list[indexX]);
        }

        #endregion

        #region 插入排序

        /// <summary>
        /// 插入排序 - 稳定排序，时间复杂度O(n²)，空间复杂度O(1)
        /// 适用于小数据量或基本有序的数据
        /// </summary>
        /// <param name="array">待排序数组</param>
        /// <param name="condition">比较条件，返回true表示需要交换</param>
        public static void InsertionSort<T>(T[] array, Func<T, T, bool> condition)
        {
            if (array == null || array.Length <= 1) return;

            for (int i = 1; i < array.Length; i++)
            {
                T current = array[i];
                int j = i - 1;

                // 从后往前比较，找到插入位置
                while (j >= 0 && condition(current, array[j]))
                {
                    array[j + 1] = array[j];
                    j--;
                }

                array[j + 1] = current;
            }
        }

        /// <summary>
        /// 插入排序 - List版本
        /// </summary>
        public static void InsertionSort<T>(List<T> list, Func<T, T, bool> condition)
        {
            if (list == null || list.Count <= 1) return;

            for (int i = 1; i < list.Count; i++)
            {
                T current = list[i];
                int j = i - 1;

                while (j >= 0 && condition(current, list[j]))
                {
                    list[j + 1] = list[j];
                    j--;
                }

                list[j + 1] = current;
            }
        }

        #endregion

        #region 归并排序

        /// <summary>
        /// 归并排序 - 稳定排序，时间复杂度O(nlogn)，空间复杂度O(n)
        /// 适用于大数据量，保证稳定性
        /// </summary>
        public static void MergeSort<T>(T[] array, Func<T, T, bool> condition)
        {
            if (array == null || array.Length <= 1) return;

            MergeSortInternal(array, 0, array.Length - 1, condition);
        }

        /// <summary>
        /// 归并排序 - List版本
        /// </summary>
        public static void MergeSort<T>(List<T> list, Func<T, T, bool> condition)
        {
            if (list == null || list.Count <= 1) return;

            MergeSortInternal(list, 0, list.Count - 1, condition);
        }

        private static void MergeSortInternal<T>(T[] array, int left, int right, Func<T, T, bool> condition)
        {
            if (left < right)
            {
                int mid = left + (right - left) / 2; // 避免整数溢出

                // 递归排序左右两部分
                MergeSortInternal(array, left, mid, condition);
                MergeSortInternal(array, mid + 1, right, condition);

                // 合并两个有序部分
                Merge(array, left, mid, right, condition);
            }
        }

        private static void MergeSortInternal<T>(List<T> list, int left, int right, Func<T, T, bool> condition)
        {
            if (left < right)
            {
                int mid = left + (right - left) / 2;

                MergeSortInternal(list, left, mid, condition);
                MergeSortInternal(list, mid + 1, right, condition);

                Merge(list, left, mid, right, condition);
            }
        }

        private static void Merge<T>(T[] array, int left, int mid, int right, Func<T, T, bool> condition)
        {
            int leftLength = mid - left + 1;
            int rightLength = right - mid;

            // 创建临时数组
            T[] leftArray = new T[leftLength];
            T[] rightArray = new T[rightLength];

            // 复制数据到临时数组
            Array.Copy(array, left, leftArray, 0, leftLength);
            Array.Copy(array, mid + 1, rightArray, 0, rightLength);

            int i = 0, j = 0, k = left;

            // 合并两个有序数组
            while (i < leftLength && j < rightLength)
            {
                if (condition(leftArray[i], rightArray[j]))
                {
                    array[k] = leftArray[i];
                    i++;
                }
                else
                {
                    array[k] = rightArray[j];
                    j++;
                }

                k++;
            }

            // 复制剩余元素
            while (i < leftLength)
            {
                array[k] = leftArray[i];
                i++;
                k++;
            }

            while (j < rightLength)
            {
                array[k] = rightArray[j];
                j++;
                k++;
            }
        }

        private static void Merge<T>(List<T> list, int left, int mid, int right, Func<T, T, bool> condition)
        {
            int leftLength = mid - left + 1;
            int rightLength = right - mid;

            List<T> leftList = new List<T>();
            List<T> rightList = new List<T>();

            for (int i = 0; i < leftLength; i++)
                leftList.Add(list[left + i]);

            for (int i = 0; i < rightLength; i++)
                rightList.Add(list[mid + 1 + i]);

            int i1 = 0, j1 = 0, k = left;

            while (i1 < leftLength && j1 < rightLength)
            {
                if (condition(leftList[i1], rightList[j1]))
                {
                    list[k] = leftList[i1];
                    i1++;
                }
                else
                {
                    list[k] = rightList[j1];
                    j1++;
                }

                k++;
            }

            while (i1 < leftLength)
            {
                list[k] = leftList[i1];
                i1++;
                k++;
            }

            while (j1 < rightLength)
            {
                list[k] = rightList[j1];
                j1++;
                k++;
            }
        }

        #endregion

        #region 冒泡排序

        /// <summary>
        /// 冒泡排序 - 稳定排序，时间复杂度O(n²)，空间复杂度O(1)
        /// 适用于小数据量或教学演示
        /// </summary>
        public static void BubbleSort<T>(T[] array, Func<T, T, bool> condition)
        {
            if (array == null || array.Length <= 1) return;

            int n = array.Length;
            bool swapped;

            for (int i = 0; i < n - 1; i++)
            {
                swapped = false;

                for (int j = 0; j < n - 1 - i; j++)
                {
                    if (condition(array[j], array[j + 1]))
                    {
                        Swap(array, j, j + 1);
                        swapped = true;
                    }
                }

                // 如果没有发生交换，说明已经有序
                if (!swapped)
                    break;
            }
        }

        /// <summary>
        /// 冒泡排序 - List版本
        /// </summary>
        public static void BubbleSort<T>(List<T> list, Func<T, T, bool> condition)
        {
            if (list == null || list.Count <= 1) return;

            int n = list.Count;
            bool swapped;

            for (int i = 0; i < n - 1; i++)
            {
                swapped = false;

                for (int j = 0; j < n - 1 - i; j++)
                {
                    if (condition(list[j], list[j + 1]))
                    {
                        Swap(list, j, j + 1);
                        swapped = true;
                    }
                }

                if (!swapped)
                    break;
            }
        }

        #endregion

        #region 基数排序

        /// <summary>
        /// 基数排序 - 稳定排序，时间复杂度O(d(n+k))，空间复杂度O(n+k)
        /// 适用于整数或字符串排序
        /// </summary>
        public static void RadixSort(int[] array)
        {
            if (array == null || array.Length <= 1) return;

            // 找到最大值
            int max = array[0];
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] > max) max = array[i];
            }

            // 对每一位进行计数排序
            for (int exp = 1; max / exp > 0; exp *= 10)
            {
                CountingSortByDigit(array, exp);
            }
        }

        private static void CountingSortByDigit(int[] array, int exp)
        {
            int n = array.Length;
            int[] output = new int[n];
            int[] count = new int[10];

            // 计数
            for (int i = 0; i < n; i++)
            {
                count[(array[i] / exp) % 10]++;
            }

            // 累加计数
            for (int i = 1; i < 10; i++)
            {
                count[i] += count[i - 1];
            }

            // 构建输出数组（从后往前保证稳定性）
            for (int i = n - 1; i >= 0; i--)
            {
                int digit = (array[i] / exp) % 10;
                output[count[digit] - 1] = array[i];
                count[digit]--;
            }

            // 复制回原数组
            Array.Copy(output, array, n);
        }

        #endregion

        #region 计数排序

        /// <summary>
        /// 计数排序 - 稳定排序，时间复杂度O(n+k)，空间复杂度O(k)
        /// 适用于整数排序，k是数据范围
        /// </summary>
        public static void CountingSort(int[] array)
        {
            if (array == null || array.Length <= 1) return;

            int n = array.Length;

            // 找到最大值和最小值
            int min = array[0], max = array[0];
            for (int i = 1; i < n; i++)
            {
                if (array[i] < min) min = array[i];
                if (array[i] > max) max = array[i];
            }

            int range = max - min + 1;
            int[] count = new int[range];
            int[] output = new int[n];

            // 计数
            for (int i = 0; i < n; i++)
            {
                count[array[i] - min]++;
            }

            // 累加计数
            for (int i = 1; i < range; i++)
            {
                count[i] += count[i - 1];
            }

            // 构建输出数组（从后往前保证稳定性）
            for (int i = n - 1; i >= 0; i--)
            {
                output[count[array[i] - min] - 1] = array[i];
                count[array[i] - min]--;
            }

            // 复制回原数组
            Array.Copy(output, array, n);
        }

        #endregion

        #region 桶排序

        /// <summary>
        /// 桶排序 - 稳定排序，时间复杂度O(n+k)，空间复杂度O(n+k)
        /// 适用于数据分布均匀的情况
        /// </summary>
        public static void BucketSort(double[] array)
        {
            if (array == null || array.Length <= 1) return;

            int n = array.Length;
            List<double>[] buckets = new List<double>[n];

            // 初始化桶
            for (int i = 0; i < n; i++)
            {
                buckets[i] = new List<double>();
            }

            // 将数据分配到桶中
            for (int i = 0; i < n; i++)
            {
                int bucketIndex = (int)(array[i] * n);
                if (bucketIndex >= n) bucketIndex = n - 1;
                buckets[bucketIndex].Add(array[i]);
            }

            // 对每个桶进行插入排序
            for (int i = 0; i < n; i++)
            {
                buckets[i].Sort();
            }

            // 合并所有桶
            int index = 0;
            for (int i = 0; i < n; i++)
            {
                foreach (double value in buckets[i])
                {
                    array[index++] = value;
                }
            }
        }

        #endregion
        
        #region 使用示例和测试方法

        /// <summary>
        /// 测试所有稳定排序算法
        /// </summary>
        public static void TestAllStableSorts()
        {
            Debug.Log("=== 测试稳定排序算法 ===");

            // 测试数据
            int[] testArray = { 64, 34, 25, 12, 22, 11, 90, 34, 25, 12 };
            int[] originalArray = (int[])testArray.Clone();

            Debug.Log($"原始数组: {string.Join(", ", testArray)}");

            // 测试插入排序
            Array.Copy(originalArray, testArray, testArray.Length);
            InsertionSort(testArray, (a, b) => a > b);
            Debug.Log($"插入排序: {string.Join(", ", testArray)}");

            // 测试归并排序
            Array.Copy(originalArray, testArray, testArray.Length);
            MergeSort(testArray, (a, b) => a > b);
            Debug.Log($"归并排序: {string.Join(", ", testArray)}");

            // 测试冒泡排序
            Array.Copy(originalArray, testArray, testArray.Length);
            BubbleSort(testArray, (a, b) => a > b);
            Debug.Log($"冒泡排序: {string.Join(", ", testArray)}");

            // 测试基数排序
            Array.Copy(originalArray, testArray, testArray.Length);
            RadixSort(testArray);
            Debug.Log($"基数排序: {string.Join(", ", testArray)}");

            // 测试计数排序
            Array.Copy(originalArray, testArray, testArray.Length);
            CountingSort(testArray);
            Debug.Log($"计数排序: {string.Join(", ", testArray)}");
        }

        #endregion
    }
}