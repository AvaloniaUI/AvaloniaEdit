<?
$population = array('New York'     => array('state' => 'NY', 'pop' => 8008278),
                    'San Antonio'  => array('state' => 'TX', 'pop' => 1144646),
                    'Detroit'      => array('state' => 'MI', 'pop' => 951270));

$state_totals = array();
$total_population = 0;

print "<table><tr><th>City</th><th>Population</th></tr>\n";
foreach ($population as $city => $info) {
    $total_population += $info['pop'];
    $state_totals[$info['state']] += $info['pop'];
    print "<tr><td>$city, {$info['state']}</td><td>{$info['pop']}</td></tr>\n";
    
}

foreach ($state_totals as $state => $pop) {
    print "<tr><td>$state</td><td>$pop</td>\n";
}
print "<tr><td>Total</td><td>$total_population</td></tr>\n";
print "</table>\n";
?>