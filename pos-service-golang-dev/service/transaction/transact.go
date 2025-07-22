package transaction

import (
	"context"
	"database/sql"
	"fmt"

	db "github.com/Delivergate-Dev/pos-service-golang/database/sqlc"
)

type queryFunc[T any] func(ctx context.Context, qtx *db.Queries) (T, error)

func New[T any](ctx context.Context, dbConn *sql.DB) (func(queryFunc[T]) (T, error), error) {
	tx, err := dbConn.Begin()
	if err != nil {
		return nil, fmt.Errorf("failed to begin transaction: %w", err)
	}

	return func(c queryFunc[T]) (T, error) {
		defer tx.Rollback()

		qtx := db.New(dbConn).WithTx(tx)
		t, err := c(ctx, qtx)
		if err != nil {
			return t, err
		}

		if err := tx.Commit(); err != nil {
			return t, fmt.Errorf("failed to commit transaction: %w", err)
		}

		return t, nil
	}, nil

}
