# TEST SETUP

## Docker command
 docker run --rm --hostname mysql --name mysql -p3306:3306 -e MYSQL_ALLOW_EMPTY_PASSWORD=yes mysql:8.0.11