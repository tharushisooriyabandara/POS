-- name: GetTenantInfo :one
SELECT host, port, database_name, username, password FROM tenants WHERE x_tenant_code = ?;

-- name: GetTenants :many
SELECT * FROM tenants;