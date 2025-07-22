package service

import (
	"context"

	"github.com/Delivergate-Dev/pos-service-golang/database"
)

type IMigrationsService interface {
	ApplyMigrations(ctx context.Context, step int) error
}

type MigrationsService struct{}

func NewMigrationsService() *MigrationsService {
	return &MigrationsService{}
}

func (s *MigrationsService) ApplyMigrations(ctx context.Context, step int) error {
	if err := database.RunTenantMigrations(ctx, step); err != nil {
		return err
	}
	return nil
}
