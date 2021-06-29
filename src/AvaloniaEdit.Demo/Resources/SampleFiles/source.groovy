class MergeSort {

    static sort(int[] data) { sort(data, 0, data.length - 1) }

    /**
     * Sorts the range data[start..end] in O(nlgn) time and O(n) space.
     */
    static sort(int[] data, int start, int end) {

        if (end > start) {

            int middle = (int) ((start + end) / 2);

            // Sort the left and right sides separately.
            sort(data, start, middle);
            sort(data, middle + 1, end);

            // Intertwine the data into one sorted list.
            mergeLists(data, start, middle, end);
        }

    }

    /**
     * Merges the two sorted sublists of data[start..middle] and data[middle+1..end].
     * O(n) time and memory.
     */
    static mergeLists(int[] data, int start, int middle, int end) {

        // Copy the left and right arrays because we'll be overwriting them.
        int[] left = Arrays.copyOfRange(data, start, middle+1)
        int[] right = Arrays.copyOfRange(data, middle+1, end+1)

        // Now, merge the lists by repeatedly adding the biggest value, from whichever list has it.
        int i = start, l = 0, r = 0; // l and r are indexes in left and right
        while (l < left.length && r < right.length)
            data[i++] = (left[l] <= right[r]) ? left[l++] : right[r++]

        // Add any leftovers on one side.
        while (l < left.length)
            data[i++] = left[l++];
        while (r < right.length)
            data[i++] = right[r++];
    }

    public static void main(def args) {
        if (args.length < 2 || !args[0].isInteger()) {
            println 'please provide an array of integers'
        } else {
            def argsint = args.collect { it as int } as int[]
            sort(argsint) 
            println argsint
        }
    }

}