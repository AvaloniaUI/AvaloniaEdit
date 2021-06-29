import sys
from functools import reduce


def bubble_sort(xs):
    def pass_list(xs):
        if len(xs) <= 1:
            return xs
        x0 = xs[0]
        x1 = xs[1]
        if x1 < x0:
            del xs[1]
            return [x1] + pass_list(xs)
        return [x0] + pass_list(xs[1:])
    return reduce(lambda acc, _: pass_list(acc), xs, xs[:])


def input_list(list_str):
    return [int(x.strip(" "), 10) for x in list_str.split(',')]


def exit_with_error():
    print('Usage: please provide a list of at least two integers to sort in the format "1, 2, 3, 4, 5"')
    sys.exit(1)


def main(args):
    try:
        xs = input_list(args[0])
        if len(xs) <= 1:
            exit_with_error()
        print(bubble_sort(xs))
    except (IndexError, ValueError):
        exit_with_error()


if __name__ == "__main__":
    main(sys.argv[1:])
