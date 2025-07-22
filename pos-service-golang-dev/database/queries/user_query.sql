-- name: GetUsers :many
SELECT 
    u.id,
    u.name,
    u.last_name,
    u.email,
    r.id as role_id,
    r.name as role_name
FROM users u
LEFT JOIN user_roles ur ON u.id = ur.user_id
LEFT JOIN roles r ON ur.role_id = r.id
ORDER BY u.name;

-- name: GetUserByEmail :one
SELECT * FROM users WHERE email = ?;

-- name: GetUserByID :one
SELECT 
    u.id,
    u.name,
    u.last_name,
    u.email,
    r.id as role_id,
    r.name as role_name
FROM users u
LEFT JOIN user_roles ur ON u.id = ur.user_id
LEFT JOIN roles r ON ur.role_id = r.id 
WHERE u.id = ?;
