#include <stdio.h>
#include <errno.h>
#include <stdlib.h>
#include <string.h>
#include <stdbool.h>

size_t parse_list(const char *orig_list, long **arr)
{
    char *list;
    char *token;
    size_t num_elements = 0;
    int i;
    int curr_index = 0;
    long temp_num;

    /* figure out the length of the list first */
    for (i = 0; orig_list[i]; i++) {
        if (orig_list[i] == ',') {
            num_elements++;
        }
    }

    /* if there are no commas, it's an invalid list */
    if (num_elements == 0) {
        *arr = NULL;
        return 0;
    }

    /* since there's one more number than there are commas, add one */
    num_elements++;

    /* alloc our array */
    *arr = malloc(num_elements * sizeof(long));

    /* find the numbers */
    list = strdup(orig_list);
    token = strtok(list, ",");
    while (token != NULL) {
        errno = 0;
        temp_num = strtol(token, NULL, 10);
        if (errno != 0) {
            *arr = NULL;
            return 0;
        }

        (*arr)[curr_index++] = temp_num;

        token = strtok(NULL, ",");
    }

    free(list);

    return num_elements;
}

void swap_elements(long *arr, size_t i, size_t j)
{
    long temp;

    temp = arr[i];
    arr[i] = arr[j];
    arr[j] = temp;
}

int bubble_sort(long *arr, size_t num_elems)
{
    int i;
    bool had_to_swap;

    while (true) {
        had_to_swap = false;

        for (i = 0; i < num_elems - 1; i++) {
            if (arr[i] > arr[i+1]) {
                swap_elements(arr, i, i+1);
                had_to_swap = true;
            }
        }

        if (!had_to_swap) {
            break;
        }
    }

    return 0;
}

void print_array(long *arr, size_t num_elems)
{
    int i;

    for (i = 0; i < num_elems - 1; i++) {
        printf("%ld, ", arr[i]);
    }

    printf("%ld\n", arr[num_elems - 1]);
}

void usage()
{
    fputs("Usage: please provide a list of at least two integers to sort in the format \"1, 2, 3, 4, 5\"\n", stderr);
}

int main(int argc, char **argv)
{
    long *arr;
    long num_elements;

    if (argc < 2) {
        usage();
        return 1;
    }

    num_elements = parse_list(argv[1], &arr);
    if (num_elements == 0) {
        usage();
        return 1;
    }

    bubble_sort(arr, num_elements);
    print_array(arr, num_elements);

    free(arr);
}
