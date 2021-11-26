(ns bubble-sort
	(:gen-class))
  
(defn- bubble [ys x]
  (if-let [y (peek ys)]
    (if (> y x)
      (conj (pop ys) x y)
      (conj ys x))
    [x]))

(defn bubble-sort [xs]
  (let [ys (reduce bubble [] xs)]
    (if (= xs ys)
      xs
      (recur ys))))

(defn- convert-to-int-array [string]
  (->> (clojure.string/split string #", " )
       (map #(Integer/parseInt %))))

(defn- convert-to-string [int-array]
  (clojure.string/join ", " int-array))

(defn- print-bubble-sort [string]   
  (println 
    (convert-to-string 
      (bubble-sort 
        (convert-to-int-array string)))))

(defn- is-valid-input [args]
  (and 
    (not= (count args) 0) 
    (not= (first args) "")
    (> (count (convert-to-int-array (first args))) 1)
    ))

(defn- print-error []
  (println "Usage: please provide a list of at least two integers to sort in the format \"1, 2, 3, 4, 5\""))

(defn main [args]
  (try
    (if (is-valid-input args) 
      (print-bubble-sort (first args)) 
      (print-error))
    (catch Exception e (print-error))))

(main *command-line-args*)